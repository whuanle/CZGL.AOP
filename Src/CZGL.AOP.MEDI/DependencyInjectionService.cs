using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AOP.MEDI
{
    public static class DependencyInjectionService
    {

        /// <summary>
        /// 使用 CZGL.AOP 处理容器中需要代理的类型
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IServiceCollection BuildAopProxy(this IServiceCollection service)
        {
            if (service is null)
                throw new ArgumentNullException(nameof(service));

            IServiceCollection proxyServiceCollection = new ServiceCollection();

            // 将容器中的设置拦截器的类型生成新的类型代理
            foreach (ServiceDescriptor item in service)
            {
                // 第一步，判断是否为继承
                // 第二步，传递暴露的接口或其它服务
                // 第三步，传递参数并生成代理类型

                // 第四步，判断作用域生命周期
                // 第五步，添加到容器中

                Type newType;
                Type serviceType = item.ServiceType;
                Type implementationType = item.ImplementationType;

                if (serviceType == implementationType)
                {
                    newType = DynamicProxy.CreateProxyClassType(item.ImplementationType);
                }
                else
                {
                    newType = DynamicProxy.CreateProxyClassType(item.ServiceType, item.ImplementationType, false);
                }
                proxyServiceCollection.Add(ServiceDescriptor.Describe(serviceType, newType, item.Lifetime));
            }
            return proxyServiceCollection;
        }

        /// <summary>
        /// 使用 CZGL.AOP 处理容器中需要代理的类型
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static ServiceCollection BuildAopProxy(this ServiceCollection service)
        {
            return (ServiceCollection)BuildAopProxy((IServiceCollection)service);
        }
    }
}
