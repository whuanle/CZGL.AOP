using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CZGL.AOP;

namespace FxDebug
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
        public Test()
        {
            Console.WriteLine("构造函数没问题");
        }
        [Log]
        public virtual void MyMethod()
        {
            Console.WriteLine("运行中");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var name = DynamicProxy.GetAssemblyName();
            var ab = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            var am = ab.DefineDynamicModule("AOPDebugModule", "AOPDebug.dll");
            DynamicProxy.SetSave(ab, am);


            ITest test1 = AopInterceptor.CreateProxyOfInterface<ITest, Test>();
            Test test2 = AopInterceptor.CreateProxyOfClass<Test>();

            ab.Save("AopDebug.dll");

            var tmp = test2.GetType();
            var tmpMethods = tmp.GetMethods();
            test1.MyMethod();
            test2.MyMethod();

            Console.ReadKey();
        }
    }
}