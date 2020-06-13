using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CZGL.AOP
{
    public class EmitHelper
    {
        /// <summary>
        /// 用于创建数组
        /// </summary>
        /// <param name="iL"></param>
        /// <param name="length">数组长度</param>
        /// <param name="type">数组类型</param>
        public static void EmitArr(ILGenerator iL, int length,Type type)
        {
            iL.Emit(OpCodes.Ldc_I4_S, length);                // 将整数值 N 作为 int32 推送到计算堆栈上。创建数组时使用
            iL.Emit(OpCodes.Newarr, type);               // 创建数组
            for (int i = 0; i < length; i++)
            {
                iL.Emit(OpCodes.Dup);   // // 复制计算堆栈上当前最顶端的值，然后将副本推送到计算堆栈上。
                iL.Emit(OpCodes.Ldc_I4, i);
                EmitLdarg(iL,i+1);
                iL.Emit(OpCodes.Stelem_Ref);
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
    }
}
