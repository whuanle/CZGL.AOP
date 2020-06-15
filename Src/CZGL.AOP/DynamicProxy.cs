using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

namespace CZGL.AOP
{
    public class DynamicProxy
    {
        private static ConcurrentDictionary<Type, Type> CacheProxyClass = new ConcurrentDictionary<Type, Type>();

        private const string _assemblyName = "AOPAssembly";
        private const string _ModuleName = "AOPModule";
        private const string _TypeName = "AOPClass";
        private static AssemblyName assemblyName;
        private static AssemblyBuilder assemblyBuilder;
        private static ModuleBuilder moduleBuilder;
        static DynamicProxy()
        {
            assemblyName = new AssemblyName(_assemblyName);
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(_ModuleName);
        }

#if DEBUG
        // 当其为 DEBUG 时，允许 .NET framework 设置导出 .dll 文件
        // 获取程序集名称
        public static AssemblyName GetAssemblyName()
        {
            return assemblyName;
        }
        // 设置保存程序集 .dll 文件
        public static void SetSave(AssemblyBuilder a, ModuleBuilder b)
        {
            assemblyBuilder = a;
            moduleBuilder = b;
        }
#endif

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TInterface">接口或类</typeparam>
        /// <typeparam name="TType">类</typeparam>
        /// <param name="parameters">构造函数参数</param>
        /// <param name="Inherit">是否为类</param>
        /// <returns></returns>
        public static TInterface CreateInterceptor<TInterface, TType>(object[] parameters = null, bool Inherit = false)
            where TInterface : class
            where TType : TInterface
        {
            Type type = typeof(TType);

            if (typeof(TType).GetCustomAttribute(typeof(InterceptorAttribute)) == null)
                return (TInterface)Activator.CreateInstance(type, parameters);

            TypeBuilder typeBuilder;
            if (Inherit)
            {
                typeBuilder = moduleBuilder.DefineType(type.Name + _TypeName, TypeAttributes.Public, type);
            }
            else
            {
                typeBuilder = moduleBuilder.DefineType(type.Name + _TypeName, TypeAttributes.Public, type, new Type[] { typeof(TInterface) });
            }

            // 判断是否为泛型，如果是则构造其为泛型
            bool isGeneric = EmitHelper.DefineGenericParameters(typeBuilder, type);

            // 生成代理类型
            Type objtype = ActionInterceptor<TType>(typeBuilder, type, Inherit);

           // 返回实例
            return Activator.CreateInstance(isGeneric ? EmitHelper.CreateDefineGeneric(objtype, type) : objtype, parameters) as TInterface;
        }

        /// <summary>
        /// 拦截 Action 并且生成类型
        /// </summary>
        /// <typeparam name="TType">被代理的类</typeparam>
        /// <param name="typeBuilder">生成器</param>
        /// <param name="parameters">构造函数参数</param>
        /// <param name="type">被代理的类的类型</param>
        /// <param name="Inherit">是否属于类继承</param>
        private static Type ActionInterceptor<TType>(TypeBuilder typeBuilder, Type type, bool Inherit)
        {
            // 生成字段，用于存放拦截的上下文信息
            FieldBuilder aspectContextField = typeBuilder.DefineField("_" + nameof(AspectContextBody), typeof(AspectContext), FieldAttributes.Private | FieldAttributes.InitOnly);

            // 存储类成员使用的特性拦截器
            Dictionary<Type, FieldBuilder> fieldList;

            var properties = type.GetProperties();
            var methods = type.GetMethods();

            fieldList = GetActionAttribute(properties, methods, typeBuilder);

            ConstructorInfo[] constructorInfos = type.GetConstructors();

            // 代理类实现与被代理类一致的构造函数
            // 代理类初始化各个拦截器
            foreach (var item in constructorInfos)
            {
                Type[] types = item.GetParameters().Select(x => x.ParameterType).ToArray();
                var constructorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    types);

                var conIL = constructorBuilder.GetILGenerator();

                if (types.Length != 0)
                    foreach (var itemtmp in types)
                    {
                        conIL.DeclareLocal(itemtmp);
                    }

                // 实例函数使用 从 Ldarg.1 开始
                // Ldarg.0 属于 this
                conIL.Emit(OpCodes.Ldarg_0);

                // 三个步骤：
                // * 处理构造函数参数
                // * 调用被代理类的构造函数并传递参数
                // * 实例化拦截器上下文、为上下文设置属性、构造拦截器

                // 如果构造函数有参数的话
                if (types.Length != 0)
                    MethodParamters(types.Length, conIL);// 处理所有参数

                // 调用父类的构造函数
                conIL.Emit(OpCodes.Call, item);

                // 实例化上下文
                conIL.Emit(OpCodes.Ldarg_0); // 将索引为 0 的自变量加载到计算堆栈上。
                conIL.Emit(OpCodes.Newobj, typeof(AspectContextBody).GetConstructor(new Type[0] { }));
                conIL.Emit(OpCodes.Stfld, aspectContextField);  // 用新值替换在对象引用或指针的字段中存储的值。

                // 为上下文设置属性
                // 设置当前代理类型
                conIL.Emit(OpCodes.Ldarg_0);
                conIL.Emit(OpCodes.Ldfld, aspectContextField);
                conIL.Emit(OpCodes.Ldarg_0);
                conIL.Emit(OpCodes.Call, type.GetMethod(nameof(GetType)));
                conIL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.Type)}"));

                // 设置构造函数参数列表

                conIL.Emit(OpCodes.Ldarg_0);
                conIL.Emit(OpCodes.Ldfld, aspectContextField);
                EmitHelper.EmitArr(conIL, types, typeof(object));
                //conIL.Emit(OpCodes.Ldc_I4_2);
                //conIL.Emit(OpCodes.Newobj,typeof(object));
                //conIL.Emit(OpCodes.Dup);
                //conIL.Emit(OpCodes.Ldarg_1);
                conIL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.ConstructorParamters)}"));


                // 实例化所有拦截器
                NewConstructor(conIL, fieldList);
                conIL.Emit(OpCodes.Ret);
            }

            // 代理拦截方法
            foreach (var item in methods)
            {
                // 是否设置了拦截器的方法
                var actionAttr = (ActionAttribute)item.GetCustomAttributes().FirstOrDefault(x => x.GetType().BaseType == typeof(ActionAttribute));
                if (actionAttr is null)
                {
                    continue;
                }

                // 获取方法的参数
                Type[] types = item.GetParameters().Select(x => x.ParameterType).ToArray();
                Type returnType = item.ReturnType;

                MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                    item.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.Standard,
                    returnType,
                    types);


                var iL = methodBuilder.GetILGenerator();

                // 生成代理方法
                MethodProxy(iL, item, aspectContextField, fieldList[actionAttr.GetType()], actionAttr);

                // 如果属于继承，则需要重写方法
                if (Inherit)
                    typeBuilder.DefineMethodOverride(methodBuilder, item);
            }

            // 代理拦截属性
            foreach (var item in properties)
            {
                // 四种情况：没有设置拦截器、属性设置拦截器、属性的get设置拦截器、属性的set设置拦截器
                // 不管构造器是否为私有都可以代理
                // 是否设置了拦截器的方法
                int mode = 0;

                MethodInfo getMethodInfo = default;
                MethodInfo setMethodInfo = default;

                // 判断属性有没有使用拦截器

                var actionAttr = (ActionAttribute)item.GetCustomAttributes().FirstOrDefault(x => x.GetType().BaseType == typeof(ActionAttribute));

                if (item.CanRead)
                    getMethodInfo = item.GetGetMethod(true);
                if (item.CanWrite)
                    setMethodInfo = item.GetSetMethod(true);


                if (actionAttr is null)
                {
                    mode = 0b000;
                    if (getMethodInfo != null)
                        if (getMethodInfo.GetCustomAttributes().FirstOrDefault(x => x.GetType().BaseType == typeof(ActionAttribute)) != null)
                            mode |= 0b001;
                    if (setMethodInfo != null)
                        if ((ActionAttribute)setMethodInfo.GetCustomAttributes().FirstOrDefault(x => x.GetType().BaseType == typeof(ActionAttribute)) != null)
                            mode |= 0b010;
                }
                else mode = 0b001 | 0b010;

                if (mode == 0b000)
                    continue;

                // 代理属性
                switch (mode)
                {
                    case 0b11:
                        GetBuilder();
                        SetBuilder();
                        break;
                    case 0b001:
                        GetBuilder();
                        break;
                    case 0b010:
                        SetBuilder();
                        break;
                }

                void GetBuilder()
                {
                    MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(
                        getMethodInfo.Name,
                        (EmitHelper.GetVisibility(getMethodInfo)) | MethodAttributes.Virtual,
                        CallingConventions.Standard,
                        getMethodInfo.ReturnType,
                        null);
                    PropertyGetProxy(setMethodBuilder.GetILGenerator(), typeof(TType), getMethodInfo, item, aspectContextField, fieldList[actionAttr.GetType()], actionAttr);
                }

                void SetBuilder()
                {
                    MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(
                        setMethodInfo.Name,
                        (EmitHelper.GetVisibility(setMethodInfo)) | MethodAttributes.Virtual,
                        CallingConventions.Standard,
                        setMethodInfo.ReturnType,
                        setMethodInfo.GetParameters().Select(x => x.ParameterType).ToArray());
                    PropertyGetProxy(getMethodBuilder.GetILGenerator(), typeof(TType), setMethodInfo, item, aspectContextField, fieldList[actionAttr.GetType()], actionAttr);
                }
            }

            return typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// 获取这个类型属性和方法使用了修饰的拦截器特性
        /// </summary>
        /// <param name="propertyInfos">被代理类的所有公开属性</param>
        /// <param name="methodInfos">被代理类的所有公开方法</param>
        /// <param name="typeBuilder">代理类的生成器</param>
        /// <returns></returns>
        private static Dictionary<Type, FieldBuilder> GetActionAttribute(PropertyInfo[] propertyInfos, MethodInfo[] methodInfos, TypeBuilder typeBuilder)
        {
            Dictionary<Type, FieldBuilder> dic = new Dictionary<Type, FieldBuilder>();

            foreach (var item in propertyInfos)
            {
                var actionAttribute = item.GetCustomAttributes().FirstOrDefault(x => x.GetType().BaseType == typeof(ActionAttribute));
                if (actionAttribute is null)
                    continue;
                var actionType = actionAttribute.GetType();
                if (dic.ContainsKey(actionType))
                    continue;
                dic.Add(actionType, typeBuilder.DefineField("_" + actionType.Name, actionType, FieldAttributes.Private | FieldAttributes.InitOnly));
            }

            foreach (var item in methodInfos)
            {
                var actionAttribute = item.GetCustomAttributes().FirstOrDefault(x => x.GetType().BaseType == typeof(ActionAttribute));
                if (actionAttribute is null)
                    continue;
                var actionType = actionAttribute.GetType();
                if (dic.ContainsKey(actionType))
                    continue;
                dic.Add(actionType, typeBuilder.DefineField("_" + actionType.Name, actionType, FieldAttributes.Private));
            }

            return dic;
        }

        /// <summary>
        /// 在构造函数中实例化拦截器
        /// </summary>
        /// <param name="il"></param>
        /// <param name="types"></param>
        private static void NewConstructor(ILGenerator il, Dictionary<Type, FieldBuilder> types)
        {
            foreach (var item in types)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Newobj, item.Key.GetConstructor(new Type[0] { }));
                il.Emit(OpCodes.Stfld, item.Value);
            }
        }


        /// <summary>
        /// 为方法、构造函数处理参数
        /// <para>只能使用 1 开始的地址，在调用此方法前，你可能需要调用 OpCodes.Ldarg_0 </para>
        /// </summary>
        /// <param name="length"></param>
        /// <param name="conIL"></param>
        private static void MethodParamters(int length, ILGenerator conIL)
        {
            switch (length)
            {
                case 0: return;
                case 1: conIL.Emit(OpCodes.Ldarg_1); return;
                case 2: conIL.Emit(OpCodes.Ldarg_1); conIL.Emit(OpCodes.Ldarg_2); return;
                case 3: conIL.Emit(OpCodes.Ldarg_1); conIL.Emit(OpCodes.Ldarg_2); conIL.Emit(OpCodes.Ldarg_3); return;
                default:
                    conIL.Emit(OpCodes.Ldarg_1); conIL.Emit(OpCodes.Ldarg_2); conIL.Emit(OpCodes.Ldarg_3);
                    for (int i = 4; i <= length; i++)
                        conIL.Emit(OpCodes.Ldarg_S, i);
                    return;
            }
        }

        /// <summary>
        /// 生成代理方法
        /// </summary>
        /// <param name="iL">IL生成器</param>
        /// <param name="methodInfo">被代理的方法</param>
        /// <param name="aspectContextField">类的上下文</param>
        /// <param name="fieldBuilder">使用的拦截器字段</param>
        /// <param name="actionAttr">使用的拦截器</param>
        private static void MethodProxy(ILGenerator iL, MethodInfo methodInfo, FieldBuilder aspectContextField, FieldBuilder fieldBuilder, ActionAttribute actionAttr)
        {
            Type returnType = methodInfo.ReturnType; // 返回类型
            Type[] types = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray(); // 参数列表
            // 五个步骤：
            // * 为上下文设置参数
            // * 执行 拦截器的 Before()
            // * 执行被代理了类的方法
            // * 执行拦截器的 After()
            // * 返回值(如果有)


            // 预先定义部分变量
            LocalBuilder tmpAspectContextBodyField = iL.DeclareLocal(typeof(AspectContextBody)); // 上下文实例，堆栈在 Stloc_0
            if (returnType != typeof(void))    // 如果有返回值时
                iL.DeclareLocal(returnType);   // 用于接收返回结果,堆栈在 Stloc_1

            // 创建一个新的上下文
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldfld, aspectContextField);
            iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"get_{nameof(AspectContextBody.NewInstance)}"));
            iL.Emit(OpCodes.Stloc_0);       // 存到堆栈 0 

            // ①
            // IsMethod = true
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Ldc_I4_1);  // 0 false 1 true
            iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.IsMethod)}"));
            // MethidInfo
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Call, typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod))); // 执行方法
            iL.Emit(OpCodes.Castclass, typeof(MethodInfo));          // 尝试将引用传递的对象转换为指定的类。
            iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.MethodInfo)}"));
            // MethodValues
            iL.Emit(OpCodes.Ldloc_0);
            EmitHelper.EmitArr(iL, types, typeof(object));
            iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.MethodValues)}"));

            // ② 方法执行前
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldfld, fieldBuilder);
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Callvirt, actionAttr.GetType().GetMethod(nameof(ActionAttribute.Before)));
            iL.Emit(OpCodes.Nop);

            // ③ 执行方法
            iL.Emit(OpCodes.Ldarg_0);
            MethodParamters(types.Length, iL);
            iL.Emit(OpCodes.Call, methodInfo);
            if (returnType == typeof(void))
                iL.Emit(OpCodes.Pop);
            else
                iL.Emit(OpCodes.Stloc_1); // 有返回值时，存到堆栈 1

            if (returnType != typeof(void))
            {
                // 将返回结果存储到上下文中的 Result 属性

                iL.Emit(OpCodes.Ldloc_0);
                iL.Emit(OpCodes.Ldloc_1);
                // 值类型需要装箱
                if (returnType.IsValueType)
                    iL.Emit(OpCodes.Box, typeof(object));

                iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.MethodResult)}"));
                iL.Emit(OpCodes.Nop);
            }


            // ④ 执行方法后
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldfld, fieldBuilder);
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Callvirt, actionAttr.GetType().GetMethod(nameof(ActionAttribute.After)));
            iL.Emit(OpCodes.Pop);

            if (returnType != typeof(void))
            {
                iL.Emit(OpCodes.Ldloc_1);       // 取出堆栈中的返回值
            }

            // ⑤
            iL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Get 代理
        /// </summary>
        private static void PropertyGetProxy(ILGenerator iL, Type type, MethodInfo getMethod, PropertyInfo propertyInfo, FieldBuilder aspectContextField, FieldBuilder fieldBuilder, ActionAttribute actionAttr)
        {
            PropertyProxy(iL, type, getMethod, propertyInfo, aspectContextField, fieldBuilder, actionAttr);
        }

        /// <summary>
        /// Set 代理
        /// </summary>
        private static void PropertySetProxy(ILGenerator iL, Type type, MethodInfo getMethod, PropertyInfo propertyInfo, FieldBuilder aspectContextField, FieldBuilder fieldBuilder, ActionAttribute actionAttr)
        {
            PropertyProxy(iL, type, getMethod, propertyInfo, aspectContextField, fieldBuilder, actionAttr);
        }

        /// <summary>
        /// 生成属性的代理方法
        /// </summary>
        /// <param name="iL">IL生成器</param>
        /// <param name="type">被代理的类</param>
        /// <param name="methodInfo">被代理的方法,Get或Set方法</param>
        /// <param name="propertyInfo">当前被代理的属性</param>
        /// <param name="aspectContextField">类的上下文</param>
        /// <param name="fieldBuilder">使用的拦截器字段</param>
        /// <param name="actionAttr">使用的拦截器</param>
        private static void PropertyProxy(ILGenerator iL, Type type, MethodInfo methodInfo, PropertyInfo propertyInfo, FieldBuilder aspectContextField, FieldBuilder fieldBuilder, ActionAttribute actionAttr)
        {
            Type returnType = methodInfo.ReturnType; // 返回类型
            bool isSet = returnType == typeof(void) ? true : false;
            Type paramTypes = methodInfo.GetParameters().Select(x => x.ParameterType).FirstOrDefault(); // 参数列表
            // 五个步骤：
            // * 为上下文设置参数
            // * 执行 拦截器的 Before()
            // * 执行被代理了类的方法
            // * 执行拦截器的 After()
            // * 返回值(如果有)


            // 预先定义部分变量
            LocalBuilder tmpAspectContextBodyField = iL.DeclareLocal(typeof(AspectContextBody)); // 上下文实例，堆栈在 Stloc_0
            if (returnType != typeof(void))    // 如果有返回值时
                iL.DeclareLocal(returnType);   // 用于接收返回结果,堆栈在 Stloc_1

            // 创建一个新的上下文
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldfld, aspectContextField);
            iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"get_{nameof(AspectContextBody.NewInstance)}"));
            iL.Emit(OpCodes.Stloc_0);       // 存到堆栈 0 

            // ①
            // IsProperty = true
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Ldc_I4_1);  // 0 false 1 true
            iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.IsProperty)}"));
            // PropertyInfo
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Call, typeof(object).GetMethod("GetType"));
            iL.Emit(OpCodes.Ldstr, propertyInfo.Name);
            iL.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(type.GetProperty), new Type[] { typeof(string) }));
            iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.PropertyInfo)}"));
            // PropertyValue
            // 如果是Set方法，那么在此处即可获得值
            if (isSet)
            {
                iL.Emit(OpCodes.Ldloc_0);
                EmitHelper.EmitOne(iL, paramTypes);
                iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.PropertyValue)}"));
            }


            // ② 方法执行前
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldfld, fieldBuilder);
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Callvirt, actionAttr.GetType().GetMethod(nameof(ActionAttribute.Before)));
            iL.Emit(OpCodes.Nop);
            iL.Emit(OpCodes.Ldarg_0);
            // ③ 执行方法
            if (isSet)
            {
                iL.Emit(OpCodes.Ldarg_1);
            }
            iL.Emit(OpCodes.Call, methodInfo);
            if (returnType != typeof(void))
                iL.Emit(OpCodes.Stloc_1); // 有返回值时，存到堆栈 1

            if (returnType != typeof(void))
            {
                // 将返回结果存储到上下文中的 Result 属性

                iL.Emit(OpCodes.Ldloc_0);
                iL.Emit(OpCodes.Ldloc_1);
                // 值类型需要装箱
                if (returnType.IsValueType)
                    iL.Emit(OpCodes.Box, typeof(object));

                iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.PropertyValue)}"));
                iL.Emit(OpCodes.Nop);
            }


            // ④ 执行方法后
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldfld, fieldBuilder);
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Callvirt, actionAttr.GetType().GetMethod(nameof(ActionAttribute.After)));
            iL.Emit(OpCodes.Pop);

            if (returnType != typeof(void))
            {
                iL.Emit(OpCodes.Ldloc_1);       // 取出堆栈中的返回值
            }

            // ⑤
            iL.Emit(OpCodes.Ret);
        }
    }
}
