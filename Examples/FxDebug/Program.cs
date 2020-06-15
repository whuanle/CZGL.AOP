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
            if (context.IsMethod)
            {
                Console.Write($"当前类型：{context.Type} 当前执行方法：{context.MethodInfo.Name} 当前方法的参数列表：");
                // 这里的示例参数都是 string，实际情况请自行判断
                foreach (var item in context.MethodValues)
                {
                    Console.Write(item);
                }
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
    public class Test<TTnt>
    {
        public Test() { }
        public Test(int a, int b)
        {
        }

        [Log] public virtual string A { get; set; }

        [Log]
        public virtual string MyMethod(string a, string b, string c, string d, string e, string f, string g, string h)
        {
            Console.WriteLine(typeof(TTnt));
            Console.WriteLine("运行中");
            return "";
        }
    }


    public class AopTest<TTnt> : Test<TTnt>
    {
        private readonly LogAttribute _LogAttribute;
        private readonly AspectContextBody _AspectContextBody;
        private readonly LogAttribute aaa;

        public AopTest(int a, int b) : base(a, b)
        {
            _AspectContextBody = new AspectContextBody();
            // 新增
            _AspectContextBody.Type = this.GetType();
            _AspectContextBody.ConstructorParamters = new object[] { a, b };
            ;
            _LogAttribute = new LogAttribute();
            aaa = new LogAttribute();
        }

        public override string A
        {
            get
            {
                AspectContextBody aspectContextBody = _AspectContextBody.NewInstance;
                aspectContextBody.IsProperty = true;
                aspectContextBody.PropertyInfo = GetType().GetProperty("A");
                _LogAttribute.Before(aspectContextBody);
                string result = base.A;
                aspectContextBody.PropertyValue = result;
                _LogAttribute.After(aspectContextBody);
                return result;
            }
            set
            {
                AspectContextBody aspectContextBody = _AspectContextBody.NewInstance;
                aspectContextBody.IsProperty = true;
                aspectContextBody.PropertyInfo = GetType().BaseType?.GetProperty("A");
                aspectContextBody.PropertyValue = value;
                _LogAttribute.Before(aspectContextBody);
                base.A = value;
                _LogAttribute.After(aspectContextBody);
            }
        }

        [Log]
        public override string MyMethod(string a, string b, string c, string d, string e, string f, string g, string h)
        {
            AspectContextBody aspectContextBody = _AspectContextBody.NewInstance;
            aspectContextBody.IsMethod = true;
            aspectContextBody.MethodInfo = (MethodInfo)MethodInfo.GetCurrentMethod();
            aspectContextBody.MethodValues = new object[] { a, b, c, d, e, f, g, h };

            _LogAttribute.Before(aspectContextBody);
            string str = base.MyMethod(a, b, c, d, e, f, g, h);
            aspectContextBody.MethodResult = str;
            _LogAttribute.After(aspectContextBody);
            return str;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var name = DynamicProxy.GetAssemblyName();
            var ab = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            var am = ab.DefineDynamicModule("AOPDebugModule", "AOPDebug1.dll");
            DynamicProxy.SetSave(ab, am);
            Test<int> test = AopInterceptor.CreateProxyOfClass<Test<int>>(1, 2);
            ab.Save("AopDebug1.dll");
            test.MyMethod("", "", "", "", "", "", "", "");
            test.A = "666";
            Console.ReadKey();
        }
    }
}