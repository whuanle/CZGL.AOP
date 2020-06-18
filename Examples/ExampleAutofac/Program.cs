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
        void MyMethod();
    }

    [Interceptor]
    public class Test : ITest
    {
        [Log]
        public virtual void MyMethod()
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
            if (context.IsMethod)
                return context.MethodResult;
            else if (context.IsProperty)
                return context.PropertyValue;
            return null;
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
                myService.MyMethod();
            }

            Console.ReadKey();
        }
    }
}
