using CZGL.AOP;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ExampleConsole
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
        void MyMethod();
    }

    [Interceptor]
    public class Test : ITest
    {
        [Log] public virtual string A { get; set; }
        [Log]
        public virtual void MyMethod()
        {
            Console.WriteLine("运行中");
        }
    }

    public class TestNo
    {
        public virtual string A { get; set; }
        public virtual void MyMethod()
        {
            Console.WriteLine("运行中");
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            ITest test1 = AopInterceptor.CreateProxyOfInterface<ITest, Test>();
            Test test2 = AopInterceptor.CreateProxyOfClass<Test>();
            test1.MyMethod();
            test2.MyMethod();

            Console.WriteLine("---");

            TestNo test3 = AopInterceptor.CreateProxyOfType<TestNo>(new ProxyTypeBuilder()
                .AddProxyMethod(typeof(LogAttribute), typeof(TestNo).GetMethod("MyMethod")));
            test3.MyMethod();
            Console.ReadKey();
        }
    }
}
