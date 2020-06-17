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
            else
                return context.PropertyValue;
        }
    }

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
            Console.WriteLine("运行中");
        }
    }

    public class TestAOPClass : Test
    {
        private readonly AspectContext _AspectContextBody;

        private LogAttribute _LogAttribute;

        public TestAOPClass()
        {
            //Error decoding local variables: Signature type sequence must have at least one element.
            _AspectContextBody = new AspectContextBody();
            ((AspectContextBody)_AspectContextBody).Type = GetType();
            ((AspectContextBody)_AspectContextBody).ConstructorParamters = new object[0];
            _LogAttribute = new LogAttribute();
        }

        public override void MyMethod()
        {
            AspectContextBody newInstance = ((AspectContextBody)_AspectContextBody).NewInstance;
            newInstance.IsMethod = true;
            newInstance.MethodInfo = (MethodInfo)MethodBase.GetCurrentMethod();
            newInstance.MethodValues = new object[0];
            _LogAttribute.Before(newInstance);
            base.MyMethod();
            _LogAttribute.After(newInstance);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var z = typeof(TestAOPClass).GetMethods();

            ITest test1 = AopInterceptor.CreateProxyOfInterface<ITest, Test>();
            Test test2 = AopInterceptor.CreateProxyOfClass<Test>();
            var zz = test1.GetType().GetMethods();
            test1.MyMethod();
            test2.MyMethod();

            Console.ReadKey();
        }
    }
}
