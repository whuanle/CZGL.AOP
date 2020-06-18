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
            // 拦截并修改方法的参数
            for (int i = 0; i < context.MethodValues.Length; i++)
            {
                context.MethodValues[i] = (int)context.MethodValues[i] + 1;
            }
            Console.WriteLine("执行前");
        }

        public override object After(AspectContext context)
        {
            Console.WriteLine("执行后");

            // 拦截方法的执行结果
            context.MethodResult = (int)context.MethodResult + 666;

            if (context.IsMethod)
                return context.MethodResult;
            else if (context.IsProperty)
                return context.PropertyValue;
            return null;
        }
    }

    [Interceptor]
    public class Test
    {
        [Log]
        public virtual int Sum(int a, int b)
        {
            Console.WriteLine("运行中");
            return a + b;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //var name = DynamicProxy.GetAssemblyName();
            //var ab = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            //var am = ab.DefineDynamicModule("AOPDebugModule", "AOPDebug.dll");
            //DynamicProxy.SetSave(ab, am);

            //ab.Save("AopDebug.dll");


            Test test = AopInterceptor.CreateProxyOfClass<Test>();

            Console.WriteLine(test.Sum(1, 1));

            Console.ReadKey();
        }
    }
}