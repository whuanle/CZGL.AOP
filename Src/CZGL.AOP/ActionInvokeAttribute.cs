using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AOP
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ActionInvokeAttribute : Attribute
    {

    }
}
