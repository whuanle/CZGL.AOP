CZGL.AOP 有五种生成代理类型的方式，下面一一介绍。

请预先创建如下代码：

```csharp
    public class LogAttribute : ActionAttribute
    {
        public override void Before(AspectContext context)
        {
            Console.WriteLine("执行前");
        }

        public override object After(AspectContext context)
        {
            Console.WriteLine("执行后");
            return context.MethodResult;
        }
    }

    public interface ITest
    {
        void MyMethod();
    }

    [Interceptor]
    public class Test:ITest
    {
        [Log]
        public void MyMethod()
        {
            Console.WriteLine("运行中");
        }
    }
```



### 1,通过API直接创建

