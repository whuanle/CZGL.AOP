using CZGL.AOP;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Xunit;

namespace AOPTest
{

    public class APITestLogAttribute : ActionAttribute
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
        [APITestLog]
        public virtual void MyMethod()
        {
            Console.WriteLine("运行中");
        }
    }

    public class Testgenerice1<T1>
    {
        public Type MyTest()
        {
            return typeof(T1);
        }
    }

    public class Testgenerice3<T1, T2, T3>
    {
        public Type[] MyTest()
        {
            return new Type[] { typeof(T1), typeof(T2), typeof(T3) };
        }
    }

    public class ApiCreateProxyTest
    {
        /// <summary>
        /// 通过接口生成代理
        /// </summary>
        [Fact]
        public void CreateProxyOfInterface()
        {
            // 通过接口
            ITest test = AopInterceptor.CreateProxyOfInterface<ITest, Test>();
            test.MyMethod();
            Assert.True(true);
        }


        /// <summary>
        /// 通过类生成代理
        /// </summary>
        [Fact]
        public void CreateProxyOfClass()
        {
            // 通过类
            Test test = AopInterceptor.CreateProxyOfClass<Test>();
            test.MyMethod();
            Assert.True(true);
        }

        /// <summary>
        /// 测试泛型类型
        /// </summary>
        [Fact]
        public void TestGenerice()
        {
            // 通过类
            var test = AopInterceptor.CreateProxyOfClass<Testgenerice1<int>>();
            bool isInt = test.MyTest() == typeof(int);

            Assert.True(isInt);

            var test1 = AopInterceptor.CreateProxyOfClass<Testgenerice1<string>>();
            bool isStr = test1.MyTest() == typeof(string);

            Assert.True(isStr);

            var test2 = AopInterceptor.CreateProxyOfClass<Testgenerice1<ApiCreateProxyTest>>();
            bool isobj = test2.MyTest() == typeof(ApiCreateProxyTest);

            Assert.True(isobj);
        }

        /// <summary>
        /// 测试泛型类型，测试多个泛型参数
        /// </summary>
        [Fact]
        public void TestGenerices()
        {
            // 通过类
            var test = AopInterceptor.CreateProxyOfClass<Testgenerice3<int, string, ApiCreateProxyTest>>();
            Type[] types = test.MyTest();
            bool isType = types[0] == typeof(int) && types[1] == typeof(string) && types[2] == typeof(ApiCreateProxyTest);

            Assert.True(isType);

            var test1 = AopInterceptor.CreateProxyOfClass<Testgenerice3<object, AopInterceptor, Type>>();
            types = test1.MyTest();
            isType = types[0] == typeof(int) && types[1] == typeof(string) && types[2] == typeof(ApiCreateProxyTest);

            Assert.True(isType);
        }

    }
}
