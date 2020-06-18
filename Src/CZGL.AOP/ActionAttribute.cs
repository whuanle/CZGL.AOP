using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AOP
{
    /// <summary>
    /// 代理
    /// <para>此特性可以拦截方法、属性</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class ActionAttribute : Attribute
    {

        public virtual void Before(AspectContext context) { }
        public virtual object After(AspectContext context)
        {
            if (context.IsMethod)
                return context.MethodResult;
            else if (context.IsProperty)
                return context.PropertyValue;
            return null;
        }
    }
}
