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
        public Test() { }
        public Test(string a, string b) { }
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
            // 通过接口
            ITest test1 = AopInterceptor.CreateProxyOfInterface<ITest, Test>();
            // 通过类
            Test test2 = AopInterceptor.CreateProxyOfClass<Test>();
            test1.MyMethod();
            test2.MyMethod();

            // 指定构造函数
            test2 = AopInterceptor.CreateProxyOfClass<Test>("aaa", "bbb");
            test2.MyMethod();

            Console.WriteLine("---");

            // 非侵入式代理
            TestNo test3 = AopInterceptor.CreateProxyOfType<TestNo>(
                new ProxyTypeBuilder(new Type[] { typeof(LogAttribute) })
                .AddProxyMethod("LogAttribute", typeof(TestNo).GetMethod(nameof(TestNo.MyMethod)))
                .AddProxyMethod("LogAttribute", typeof(TestNo).GetProperty(nameof(TestNo.A)).GetSetMethod()));

            test3.MyMethod();
            test3.A = "666";

            Console.ReadKey();
        }
    }
}
