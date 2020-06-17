using System;
using System.Reflection.Metadata;
using Autofac;
using Autofac.Core;
using CZGL.AOP;
using CZGL.AOP.Autofac;

namespace ExampleAutofac
{
    public interface ITest
    {
        void TLine();
    }
    [Interceptor]
    public class Test : ITest
    {
        [Log]
        public void TLine()
        {
            Console.WriteLine("执行中");
        }
    }
    
    public class LogAttribute : ActionAttribute
    {
        public override void Before(AspectContext context)
        {
            Console.WriteLine("执行前");
        }
        public override object After(AspectContext context)
        {
            Console.WriteLine("执行后");
            return default;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<Test>().As<ITest>();
            var container = builder.Build().BuildAopProxy();
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                // 获取实例
                ITest myService = scope.Resolve<ITest>();
                myService.TLine();
            }
            Console.WriteLine("Hello World!");
        }
    }
}
