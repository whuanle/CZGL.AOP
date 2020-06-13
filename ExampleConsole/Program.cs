using CZGL.AOP;
using System;
using System.Linq;
using System.Reflection;

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
            return default;
        }
    }


    [Interceptor]
    public class Test
    {
        public string A { 
            get; 
            set; }
        [Log]
        public void MyMethod()
        {
            Console.WriteLine("运行中");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            Test test = AopInterceptor.CreateProxyOfClass<Test>();
            test.MyMethod();
            Console.ReadKey();
        }
    }
}
