using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace CZGL.AOP
{
    public class EmitHelper
    {
        /// <summary>
        /// 用于创建数组比推送到计算堆栈上
        /// </summary>
        /// <param name="iL"></param>
        /// <param name="params">要传递的参数数组</param>
        /// <param name="type">需要创建的数组类型</param>
        /// <param name="isUnBox">是否拆箱<para>默认下，创建的数组是引用类型的数组如object，那么可能出现装箱操作；如果创建的是值类型数组，则可能发生拆箱操作</para></param>
        public static void EmitArr(ILGenerator iL, ParameterInfo[] @params, Type type, bool isUnBox = false)
        {
            int length = @params.Length;

            iL.Emit(OpCodes.Ldc_I4, length);                // 将整数值 N 作为 int32 推送到计算堆栈上，数组的长度
            iL.Emit(OpCodes.Newarr, type);               // 创建数组
            // 长度为 0 的数组
            if (length == 0)
                return;
            for (int i = 0; i < length; i++)
            {
                Type paramType = @params[i].ParameterType;
                iL.Emit(OpCodes.Dup);   // // 复制计算堆栈上当前最顶端的值，然后将副本推送到计算堆栈上。
                iL.Emit(OpCodes.Ldc_I4, i);
                EmitLdarg(iL, i + 1);

                // 拆箱，当前属于值类型数组，因为反射传递的参数属于object
                if (isUnBox)
                {
                    if (!paramType.IsValueType)
                        iL.Emit(OpCodes.Unbox, paramType);
                }
                // 装箱
                else
                {
                    if (paramType.IsValueType)
                        iL.Emit(OpCodes.Box, paramType);
                    else if (paramType != typeof(object))
                        iL.Emit(OpCodes.Castclass,paramType);
                }

                if (paramType.IsByRef)       // ref 或 out
                    iL.Emit(OpCodes.Ldind_Ref);

                iL.Emit(OpCodes.Stelem_Ref);
            }
        }

        /// <summary>
        /// 用于创建一个参数比推送到计算堆栈上
        /// </summary>
        /// <param name="iL"></param>
        /// <param name="paramType">要传递的参数数组</param>
        /// <param name="type">需要创建的数组类型</param>
        /// <param name="isUnBox">是否拆箱<para>默认下，创建的数组是引用类型的数组如object，那么可能出现装箱操作；如果创建的是值类型数组，则可能发生拆箱操作</para></param>
        public static void EmitOne(ILGenerator iL, Type paramType, bool isUnBox = false)
        {
            iL.Emit(OpCodes.Ldarg_1);
            // 拆箱
            if (isUnBox)
            {
                if (!paramType.IsValueType)
                    iL.Emit(OpCodes.Unbox, paramType);
            }
            // 装箱
            else
            {
                if (paramType.IsValueType)
                    iL.Emit(OpCodes.Box, paramType);
                else if (paramType != typeof(object))
                    iL.Emit(OpCodes.Castclass, paramType);
            }
        }

        /// <summary>
        /// 将索引为 n 的自变量加载到计算堆栈上。
        /// </summary>
        /// <param name="iL"></param>
        /// <param name="n"></param>
        public static void EmitLdarg(ILGenerator iL, int n)
        {
            switch (n)
            {
                case 0: iL.Emit(OpCodes.Ldarg_0); return;
                case 1: iL.Emit(OpCodes.Ldarg_1); return;
                case 2: iL.Emit(OpCodes.Ldarg_2); return;
                case 3: iL.Emit(OpCodes.Ldarg_3); return;
                default:
                    iL.Emit(OpCodes.Ldarg_S, n);
                    return;
            }
        }

        /// <summary>
        /// 获取成员方法的访问修饰符
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static MethodAttributes GetVisibility(MethodInfo method)
        {
            return
                method.IsPublic ? MethodAttributes.Public :
                method.IsPrivate ? MethodAttributes.Private :
                method.IsAssembly ? MethodAttributes.Assembly :
                method.IsFamily ? MethodAttributes.Family :
                method.IsFamilyOrAssembly ? MethodAttributes.FamORAssem : throw new Exception("无法识别此属性或方法的访问修饰符！");
        }

        /// <summary>
        /// 定义一个泛型类型
        /// <para>如果发现并不需要定义泛型，则不会操作</para>
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="paramterType">要生成的相应类型的泛型参数列表</param>
        /// <returns>是否为泛型</returns>
        public static bool DefineGenericParameters(TypeBuilder typeBuilder, Type paramterType)
        {
            if (!paramterType.IsGenericType)
                return false;
            if (paramterType.IsGenericTypeDefinition)
                throw new ArgumentException($"无法创建 {paramterType.FullName} 的实例，因为未设置此泛型的参数。");
            typeBuilder.DefineGenericParameters(paramterType.GetGenericArguments().Select(x => x.Name).ToArray());
            return true;
        }

        /// <summary>
        /// 创建一个泛型类型
        /// </summary>
        /// <param name="type">泛型类型</param>
        /// <param name="parentType">父类型</param>
        public static Type CreateGenericClass(Type type, Type parentType)
        {
            return type.MakeGenericType(parentType.GetGenericArguments());
        }

        /// <summary>
        /// 创建泛型方法
        /// </summary>
        /// <param name="methodBuilder"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static bool CreateGenericMethod(MethodBuilder methodBuilder, MethodInfo methodInfo)
        {
            if (!methodInfo.IsGenericMethod)
                return false;
            // 动态构建泛型方法时，不需要预先定义泛型参数
            //if (methodInfo.IsGenericMethodDefinition)
            //    throw new ArgumentException($"无法创建 {methodInfo.Name} 的泛型方法，因为未设置此泛型的参数。");
            methodBuilder.DefineGenericParameters(methodInfo.GetGenericArguments().Select(x => x.Name).ToArray());
            return true;
        }
    }
}
