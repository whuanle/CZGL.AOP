using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CZGL.AOP
{
    /// <summary>
    /// 生成动态代理
    /// </summary>
    public class DynamicProxy
    {
        /// <summary>
        /// 用于缓存已经生成过的代理类型
        /// </summary>
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

        #region
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
        #endregion

        #region 生成代理类型
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

            // 生成代理类型
            Type objtype = CreateProxyClassType<TInterface, TType>(Inherit);

            // 返回实例
            return Activator.CreateInstance(type.IsGenericType ? EmitHelper.CreateGenericClass(objtype, type) : objtype, parameters) as TInterface;
        }

        /// <summary>
        ///  创建非侵入式代理类型
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="implementationType"></param>
        /// <param name="noAction"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static TType CreateInterceptor<TType>(Type implementationType, NoActionAttributeModel noAction, params object[] parameters)
            where TType : class
        {
            // 生成代理类型
            Type objtype = CreateProxyClassTypeNoAttribute(implementationType, noAction);

            // 返回实例
            return Activator.CreateInstance(implementationType.IsGenericType ? EmitHelper.CreateGenericClass(objtype, implementationType) : objtype, parameters) as TType;
        }

        /// <summary>
        /// 创建代理类型
        /// </summary>
        /// <param name="implementationType">当前类型</param>
        /// <param name="Inherit">是否直接继承类型</param>
        /// <returns></returns>
        public static Type CreateProxyClassType(Type implementationType)
        {
            return CreateProxyClassType(implementationType, implementationType, true);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TType"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="Inherit"></param>
        /// <returns></returns>
        public static Type CreateProxyClassType<TInterface, TType>(bool Inherit = false)
            where TInterface : class
            where TType : TInterface
        {
            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TType);

            return CreateProxyClassType(interfaceType, implementationType, Inherit);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <param name="interfaceType">实现的接口或继承</param>
        /// <param name="implementationType">实现的类型</param>
        /// <param name="Inherit">是否通过继承生成</param>
        /// <returns></returns>
        public static Type CreateProxyClassType(Type interfaceType, Type implementationType, bool Inherit = false)
        {
            // ASP.NET Core 中，可能只有接口
            if (implementationType == null)
                return null;

            Type type = implementationType;

            if (implementationType.GetCustomAttribute(typeof(InterceptorAttribute)) == null)
                return type;

            TypeBuilder typeBuilder;
            if (Inherit)
            {
                if (CacheProxyClass.ContainsKey(implementationType))
                    return CacheProxyClass[type];
                typeBuilder = moduleBuilder.DefineType("CZGLAOP." + type.Name + _TypeName, type.Attributes, type);
            }
            else
            {
                if (CacheProxyClass.ContainsKey(implementationType))
                    return CacheProxyClass[interfaceType];
                typeBuilder = moduleBuilder.DefineType("CZGLAOP." + type.Name + _TypeName, type.Attributes, type);
            }

            // 判断是否为泛型，如果是则构造其为泛型
            bool isGeneric = EmitHelper.DefineGenericParameters(typeBuilder, type);

            // 生成代理类型
            Type proxyType = ActionInterceptor(type, typeBuilder, Inherit);
            CacheProxyClass.TryAdd(implementationType, proxyType);
            return proxyType;
        }



        /// <summary>
        /// 通过非侵入式来生成代理类型
        /// </summary>
        /// <param name="interfaceType">实现的接口或继承</param>
        /// <param name="implementationType">实现的类型</param>
        /// <param name="Inherit">是否通过继承生成</param>
        /// <returns></returns>
        public static Type CreateProxyClassTypeNoAttribute(Type implementationType, NoActionAttributeModel noAction)
        {
            Type type = implementationType;
            TypeBuilder typeBuilder;
            if (CacheProxyClass.ContainsKey(implementationType))
                return CacheProxyClass[type];
            typeBuilder = moduleBuilder.DefineType("CZGLAOP." + type.Name + _TypeName, type.Attributes, type);

            // 判断是否为泛型，如果是则构造其为泛型
            bool isGeneric = EmitHelper.DefineGenericParameters(typeBuilder, type);

            // 生成代理类型
            Type proxyType = ActionInterceptor(type, typeBuilder, true, noAction);
            CacheProxyClass.TryAdd(implementationType, proxyType);
            return proxyType;
        }

        #endregion

        #region 拦截代理类型

        /// <summary>
        /// 拦截 Action 并且生成类型
        /// </summary>
        /// <typeparam name="TType">被代理的类</typeparam>
        /// <param name="typeBuilder">生成器</param>
        /// <param name="parameters">构造函数参数</param>
        /// <param name="type">被代理的类的类型</param>
        /// <param name="Inherit">是否属于类继承</param>
        public static Type ActionInterceptor<TType>(TypeBuilder typeBuilder, bool Inherit)
        {
            return ActionInterceptor(typeof(TType), typeBuilder, Inherit);
        }

        /// <summary>
        /// 拦截 Action 并且生成类型
        /// </summary>
        /// <typeparam name="TType">被代理的类</typeparam>
        /// <param name="parentType">父类</param>
        /// <param name="typeBuilder">生成器</param>
        /// <param name="Inherit">是否属于类继承</param>
        /// <param name="noAction">非嵌入式代理时</param>
        public static Type ActionInterceptor(Type parentType, TypeBuilder typeBuilder, bool Inherit, NoActionAttributeModel noAction = null)
        {
            // 生成字段，用于存放拦截的上下文信息
            FieldBuilder aspectContextField = typeBuilder.DefineField("_" + nameof(AspectContextBody), typeof(AspectContext), FieldAttributes.Private | FieldAttributes.InitOnly);

            // 存储类成员使用的特性拦截器
            Dictionary<Type, FieldBuilder> fieldList;

            var properties = parentType.GetProperties();
            var methods = parentType.GetMethods();

            fieldList = noAction == null ? GetActionAttribute(properties, methods, typeBuilder) : GetActionAttribute(typeBuilder, noAction);

            ConstructorInfo[] constructorInfos = parentType.GetConstructors();

            // 代理类实现与被代理类一致的构造函数
            // 代理类初始化各个拦截器
            foreach (var item in constructorInfos)
            {
                ParameterInfo[] paramTypes = item.GetParameters();
                var constructorBuilder = typeBuilder.DefineConstructor(
                    item.Attributes,
                    item.CallingConvention,
                    paramTypes.Select(x => x.ParameterType).ToArray());

                var conIL = constructorBuilder.GetILGenerator();

                if (paramTypes.Length != 0)
                    foreach (var itemtmp in paramTypes)
                    {
                        conIL.DeclareLocal(itemtmp.ParameterType);
                    }

                // 实例函数使用 从 Ldarg.1 开始
                // Ldarg.0 属于 this
                conIL.Emit(OpCodes.Ldarg_0);

                // 三个步骤：
                // * 处理构造函数参数
                // * 调用被代理类的构造函数并传递参数
                // * 实例化拦截器上下文、为上下文设置属性、构造拦截器

                // 如果构造函数有参数的话
                if (paramTypes.Length > 0)
                    MethodParamters(paramTypes.Length, conIL);// 处理所有参数

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
                conIL.Emit(OpCodes.Castclass, typeof(AspectContextBody));
                conIL.Emit(OpCodes.Ldarg_0);
                conIL.Emit(OpCodes.Call, parentType.GetMethod(nameof(GetType)));
                conIL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.Type)}"));

                // 设置构造函数参数列表
                conIL.Emit(OpCodes.Ldarg_0);
                conIL.Emit(OpCodes.Ldfld, aspectContextField);
                conIL.Emit(OpCodes.Castclass, typeof(AspectContextBody));
                EmitHelper.EmitArr(conIL, paramTypes, typeof(object));
                conIL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.ConstructorParamters)}"));



                // 实例化所有拦截器
                NewConstructor(conIL, fieldList);
                conIL.Emit(OpCodes.Ret);
            }

            // 代理拦截方法
            foreach (var item in methods)
            {
                // 判断是否为侵入式代理
                // 是否设置了拦截器的方法
                ActionAttribute actionAttr;
                Type actionAttrType;
                if (noAction == null)
                {
                    actionAttr = (ActionAttribute)item.GetCustomAttributes().FirstOrDefault(x => x.GetType().BaseType == typeof(ActionAttribute));
                    if (actionAttr is null)
                        continue;
                    actionAttrType = actionAttr.GetType();
                }
                else
                {
                    string actionName;
                    if (!noAction.MethodNames.TryGetValue(item, out actionName))
                        continue;
                    actionAttrType = noAction.Actions[actionName];
                }

                // 获取方法的参数
                Type[] types = item.GetParameters().Select(x => x.ParameterType).ToArray();
                Type returnType = item.ReturnType;

                MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                    item.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    CallingConventions.Standard,
                    returnType,
                    types);

                // 判断是否为泛型方法并生成泛型类型
                bool isGeneric = EmitHelper.CreateGenericMethod(methodBuilder, item);

                var iL = methodBuilder.GetILGenerator();

                // 生成代理方法
                MethodProxy(iL, item, aspectContextField, fieldList[actionAttrType], actionAttrType);

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
                        MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                        getMethodInfo.CallingConvention,
                        getMethodInfo.ReturnType,
                        null);
                    PropertyGetProxy(setMethodBuilder.GetILGenerator(), parentType, getMethodInfo, item, aspectContextField, fieldList[actionAttr.GetType()], actionAttr);
                }

                void SetBuilder()
                {
                    MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(
                        setMethodInfo.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                        setMethodInfo.CallingConvention,
                        setMethodInfo.ReturnType,
                        setMethodInfo.GetParameters().Select(x => x.ParameterType).ToArray());
                    PropertyGetProxy(getMethodBuilder.GetILGenerator(), parentType, setMethodInfo, item, aspectContextField, fieldList[actionAttr.GetType()], actionAttr);
                }
            }

            return typeBuilder.CreateTypeInfo();
        }

        #endregion

        /// <summary>
        /// 获取这个类型属性和方法使用了修饰的拦截器特性
        /// </summary>
        /// <param name="propertyInfos">被代理类的所有公开属性</param>
        /// <param name="methodInfos">被代理类的所有公开方法</param>
        /// <param name="typeBuilder">代理类的生成器</param>
        /// <returns></returns>
        public static Dictionary<Type, FieldBuilder> GetActionAttribute(PropertyInfo[] propertyInfos, MethodInfo[] methodInfos, TypeBuilder typeBuilder)
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
                dic.Add(actionType, typeBuilder.DefineField("_" + actionType.Name, actionType, FieldAttributes.Private | FieldAttributes.InitOnly));
            }

            return dic;
        }

        public static Dictionary<Type, FieldBuilder> GetActionAttribute(TypeBuilder typeBuilder, NoActionAttributeModel noAction)
        {
            Dictionary<Type, FieldBuilder> dic = new Dictionary<Type, FieldBuilder>();

            foreach (var item in noAction.Actions)
            {
                var actionType = item.Value;
                dic.Add(actionType, typeBuilder.DefineField("_" + actionType.Name, actionType, FieldAttributes.Private | FieldAttributes.InitOnly));
            }
            return dic;
        }

        /// <summary>
        /// 在构造函数中实例化拦截器
        /// </summary>
        /// <param name="il"></param>
        /// <param name="types"></param>
        public static void NewConstructor(ILGenerator il, Dictionary<Type, FieldBuilder> types)
        {
            foreach (var item in types)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Newobj, item.Key.GetConstructor(new Type[0] { }));
                il.Emit(OpCodes.Stfld, item.Value);
            }
        }


        /// <summary>
        /// 用于拦截方法参数，然后从上下文中传递过去
        /// <para></para>
        /// </summary>
        /// <param name="methoded">被代理的方法</param>
        /// <param name="conIL"></param>
        /// <param name="contextMethod">上下文存储方法参数的属性的get方法</param>
        public static void MethodProxyParamters(MethodInfo methoded, ILGenerator conIL, MethodInfo contextMethod)
        {
            Type[] paramTypes = methoded.GetParameters().Select(x => x.ParameterType).ToArray();
            int length = paramTypes.Length;
            for (int i = 0; i < length; i++)
            {
                if (paramTypes[i].IsByRef)
                {
                    MethodParamters(i + 1, conIL);
                    continue;
                }
                conIL.Emit(OpCodes.Ldloc_0);    // aspectContextBody
                conIL.Emit(OpCodes.Callvirt, contextMethod);   // 上下文存放方法参数的数组
                conIL.Emit(OpCodes.Ldc_I4, i);           // 取出数组第 0 位数据
                conIL.Emit(OpCodes.Ldelem_Ref);         // 将位于指定数组索引处的包含对象引用的元素作为 O 类型（对象引用）加载到计算堆栈的顶部。

                if (paramTypes[i].IsValueType)   // 参数是值类型，需要拆箱
                    conIL.Emit(OpCodes.Unbox_Any, paramTypes[i]);     // 将值类型的已装箱的表示形式转换为其未装箱的形式。
                else if (paramTypes[i] != typeof(object))
                    conIL.Emit(OpCodes.Castclass, paramTypes[i]);   // 将引用类型转为另一个引用类型

            }
        }

        /// <summary>
        /// 为方法、构造函数处理参数
        /// <para>只能使用 1 开始的地址，在调用此方法前，你可能需要调用 OpCodes.Ldarg_0 </para>
        /// </summary>
        /// <param name="length"></param>
        /// <param name="conIL"></param>
        public static void MethodParamters(int length, ILGenerator conIL)
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
        public static void MethodProxy(ILGenerator iL, MethodInfo methodInfo, FieldBuilder aspectContextField, FieldBuilder fieldBuilder, Type actionAttr)
        {
            Type returnType = methodInfo.ReturnType; // 返回类型
            ParameterInfo[] types = methodInfo.GetParameters(); // 参数列表
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
            iL.Emit(OpCodes.Ldarg_0);       // this
            iL.Emit(OpCodes.Ldfld, aspectContextField); // _AspectContextBody
            iL.Emit(OpCodes.Castclass, typeof(AspectContextBody));
            iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"get_{nameof(AspectContextBody.NewInstance)}")); // NewInstance
            iL.Emit(OpCodes.Stloc_0);       // 调用 aspectContextBody 的 NewInstance 属性

            // ①
            // IsMethod = true
            iL.Emit(OpCodes.Ldloc_0);   // aspectContextBody
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
            iL.Emit(OpCodes.Callvirt, actionAttr.GetMethod(nameof(ActionAttribute.Before)));
            iL.Emit(OpCodes.Nop);

            // ③ 执行方法
            if (returnType != typeof(void))
                iL.Emit(OpCodes.Ldloc_0); // aspectContextBody

            iL.Emit(OpCodes.Ldarg_0);
            // 如果此方法没有参数
            if (types.Length == 0)
                MethodParamters(types.Length, iL);
            else
            {
                MethodProxyParamters(methodInfo, iL, typeof(AspectContextBody).GetMethod($"get_{nameof(AspectContextBody.MethodValues)}"));
            }
            iL.Emit(OpCodes.Call, methodInfo);

            if (returnType != typeof(void))
            {
                // 将返回结果存储到上下文中

                // 值类型需要装箱
                if (returnType.IsValueType)
                    iL.Emit(OpCodes.Box, returnType);
                iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.MethodResult)}"));
            }


            // ④ 执行方法后
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldfld, fieldBuilder);
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Callvirt, actionAttr.GetMethod(nameof(ActionAttribute.After)));
            if (returnType != typeof(void))
            {
                if (returnType.IsValueType)
                    iL.Emit(OpCodes.Unbox_Any, methodInfo.ReturnType);
                else if (returnType != typeof(object))
                    iL.Emit(OpCodes.Castclass, methodInfo.ReturnType);
                iL.Emit(OpCodes.Stloc_1);
                iL.Emit(OpCodes.Ldloc_1);       // 取出堆栈中的返回值
            }
            else
            {
                iL.Emit(OpCodes.Pop);
            }
            // ⑤
            iL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Get 代理
        /// </summary>
        public static void PropertyGetProxy(ILGenerator iL, Type type, MethodInfo getMethod, PropertyInfo propertyInfo, FieldBuilder aspectContextField, FieldBuilder fieldBuilder, ActionAttribute actionAttr)
        {
            PropertyProxy(iL, type, getMethod, propertyInfo, aspectContextField, fieldBuilder, actionAttr);
        }
        /// <summary>
        /// Set 代理
        /// </summary>
        public static void PropertySetProxy(ILGenerator iL, Type type, MethodInfo getMethod, PropertyInfo propertyInfo, FieldBuilder aspectContextField, FieldBuilder fieldBuilder, ActionAttribute actionAttr)
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
        public static void PropertyProxy(ILGenerator iL, Type type, MethodInfo methodInfo, PropertyInfo propertyInfo, FieldBuilder aspectContextField, FieldBuilder fieldBuilder, ActionAttribute actionAttr)
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
            // ③ 执行方法
            if (isSet)
            {
                iL.Emit(OpCodes.Ldarg_0);
                iL.Emit(OpCodes.Ldloc_0);
                iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"get_{nameof(AspectContextBody.PropertyValue)}"));
                if (returnType.IsValueType)
                    iL.Emit(OpCodes.Box, methodInfo.GetParameters().Select(x => x.ParameterType).First());
                else if (returnType != typeof(object))
                    iL.Emit(OpCodes.Castclass, methodInfo.GetParameters().Select(x => x.ParameterType).First());
            }
            else
            {
                iL.Emit(OpCodes.Ldloc_0);
                iL.Emit(OpCodes.Ldarg_0);
            }
            iL.Emit(OpCodes.Call, methodInfo);


            // 有返回值时，使用上下文存储
            if (returnType != typeof(void))
            {
                // 值类型需要装箱
                if (returnType.IsValueType)
                    iL.Emit(OpCodes.Box, returnType);
                else if (returnType != typeof(object))
                    iL.Emit(OpCodes.Castclass, returnType);
                iL.Emit(OpCodes.Callvirt, typeof(AspectContextBody).GetMethod($"set_{nameof(AspectContextBody.PropertyValue)}"));

            }


            // ④ 执行方法后
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldfld, fieldBuilder);
            iL.Emit(OpCodes.Ldloc_0);
            iL.Emit(OpCodes.Callvirt, actionAttr.GetType().GetMethod(nameof(ActionAttribute.After)));

            if (returnType != typeof(void))
            {
                if (returnType.IsValueType)
                    iL.Emit(OpCodes.Unbox_Any, methodInfo.ReturnType);
                else
                    iL.Emit(OpCodes.Castclass, methodInfo.ReturnType);
                iL.Emit(OpCodes.Stloc_1);
                iL.Emit(OpCodes.Ldloc_1);       // 取出堆栈中的返回值
            }
            else
            {
                iL.Emit(OpCodes.Pop);
            }
            // ⑤
            iL.Emit(OpCodes.Ret);
        }

    }
}
