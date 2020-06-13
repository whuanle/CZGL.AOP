using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using CZGL.AOP;

namespace FxDebug
{
    public class LogAttribute : ActionAttribute
    {
        public override void Before(AspectContext context)
        {
            Console.Write($"当前类型：{context.Type} 当前执行方法：{context.MethodInfo.Name} 当前方法的参数列表：");
            // 这里的示例参数都是 string，实际情况请自行判断
            foreach (var item in context.MethodValues)
            {
                Console.Write(item);
            }
            Console.WriteLine();
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
        public string A { get; set; }

        [Log]
        public virtual string MyMethod(string a, string b, string c, string d, string e, string f, string g, string h)
        {
            Console.WriteLine("运行中");
            return "";
        }
    }


    public class AopTest : Test
    {
        private LogAttribute _LogAttribute;
        private AspectContextBody _AspectContextBody;

        public AopTest() : base()
        {
            _AspectContextBody = new AspectContextBody();
            // 新增
            _AspectContextBody.Type = this.GetType();
            _AspectContextBody.ConstructorParamters = new object[] { };
            _LogAttribute = new LogAttribute();
        }

        public string A { get; set; }

        [Log]
        public override string MyMethod(string a, string b, string c, string d, string e, string f, string g, string h)
        {
            _AspectContextBody.IsMethod = true;
            _AspectContextBody.MethodInfo = (MethodInfo)MethodBase.GetCurrentMethod();
            _AspectContextBody.MethodValues = new object[] { a, b, c, d, e, f, g, h };

            _LogAttribute.Before(_AspectContextBody);
            string str=base.MyMethod(a, b, c, d, e, f, g, h);
            _AspectContextBody.Result = str;
            _LogAttribute.After(_AspectContextBody);
            return str;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            // Test a = new AopTest() as Test;

            // a.MyMethod("", "", "", "", "", "", "", "");

            var name = DynamicProxy.GetAssemblyName();
            var ab = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            var am = ab.DefineDynamicModule("AOPDebugModule", "AOPDebug1.dll");
            DynamicProxy.SetSave(ab, am);
            Test test = AopInterceptor.CreateProxyOfClass<Test>();
            ab.Save("AopDebug1.dll");
            test.MyMethod("", "", "", "", "", "", "", "");
            Console.ReadKey();
        }
    }
}