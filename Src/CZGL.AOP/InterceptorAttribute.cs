using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AOP
{
    /// <summary>
    /// 拦截器
    /// <para>在需要拦截的类型中使用</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InterceptorAttribute : Attribute
    {
    }
}
