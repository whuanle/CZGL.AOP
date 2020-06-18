using CZGL.AOP;
using System;
using System.Collections.Generic;

namespace ExampleMEDI
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
        public Test(int a, int b)
        {
        }

        [Log] public virtual List<string> A { get; set; }

        [Log]
        public virtual string MyMethod(string a, string b)
        {
            Console.WriteLine("运行中");
            return "";
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            // 使用Microsoft.Extensions.DependencyInjection依赖注入框架
            DependencyInjectionBuilder builder = new DependencyInjectionBuilder();
            builder.AddService<ITest, Test>();
            IServiceProvider service = builder.Build();


            ITest obj = service.Get<ITest>();
            obj.MyMethod("", "");
            Console.ReadKey();
        }
    }
}
