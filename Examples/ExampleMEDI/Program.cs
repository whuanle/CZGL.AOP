using CZGL.AOP;
using System;
using System.Collections.Generic;

namespace ExampleMEDI
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
    public interface ITest
    {
        string MyMethod(string a, string b);
    }
    [Interceptor]
    public class Test : ITest
    {
        public Test()
        {
            Console.WriteLine("父类的构造函数");
        }
        public Test(int a, int b)
        {
        }

        [Log] public virtual List<string> A { get; set; }

        [Log]
        public virtual string MyMethod(string a, string b)
        {
            Console.WriteLine("运行中");
            return "";
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            DependencyInjectionBuilder builder = new DependencyInjectionBuilder();
            builder.AddService<ITest, Test>();
            var service = builder.Build();
            ITest obj = service.Get<ITest>();
            obj.MyMethod("","");
            Console.ReadKey();
        }
    }
}
