using CZGL.AOP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleWebMEDI
{
    public class LogAttribute : ActionAttribute
    {
        public override void Before(AspectContext context)
        {
            Console.WriteLine("执行前");
        }

        public override object After(AspectContext context)
        {
            Console.WriteLine("执行后");
            if (context.IsMethod)
                return context.MethodResult;
            else if (context.IsProperty)
                return context.PropertyValue;
            return null;
        }
    }
    public interface ITest
    {
        string MyMethod(string a, string b);
    }
    [Interceptor]
    public class Test : ITest
    {
        public Test()
        {
            Console.WriteLine("父类的构造函数");
        }
        [Log] public virtual List<string> A { get; set; }

        [Log]
        public virtual string MyMethod(string a, string b)
        {
            Console.WriteLine("运行中");
            return "";
        }
    }

}
