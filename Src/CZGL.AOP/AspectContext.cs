using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace CZGL.AOP
{
    /// <summary>
    /// 拦截器上下文
    /// </summary>
    public interface AspectContext
    {
        /// <summary>
        /// 当前被代理类的类型
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// 构造函数传递的参数列表
        /// <para>如果此方法没有参数，则 MethodValues.Length = 0，而不是为 null </para>
        /// </summary>
        object[] ConstructorParamters { get; }

        /// <summary>
        /// 拦截的是否为属性
        /// </summary>
        bool IsProperty { get; }

        /// <summary>
        /// 获取运行的属性信息
        /// </summary>
        PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// 获取拦截的属性值，get 或 set 时的值
        /// </summary>
        object PropertyValue { get; }

        /// <summary>
        /// 当前代理的是否为方法
        /// </summary>
        bool IsMethod { get; }

        /// <summary>
        /// 获取运行的方法信息
        /// </summary>
        MethodInfo MethodInfo { get; }

        /// <summary>
        /// 方法传递的参数
        /// <para>如果此方法没有参数，则 MethodValues.Length = 0，而不是为 null </para>
        /// </summary>
        object[] MethodValues { get; }

        /// <summary>
        /// 方法执行返回的结果(如果有)
        /// </summary>
        object Result { get; }
    }

    public class AspectContextBody : AspectContext
    {
        public Type Type { get; set; }

        public object[] ConstructorParamters { get; set; }

        public bool IsProperty { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public object PropertyValue { get; set; }

        public bool IsMethod { get; set; }

        public MethodInfo MethodInfo { get; set; }

        public object[] MethodValues { get; set; }

        public object Result { get; set; }
    }

}
