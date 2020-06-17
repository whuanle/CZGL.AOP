using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CZGL.AOP.Autofac
{
    public static class DependencyInjectionService
    {

        /// <summary>
        /// 使用 CZGL.AOP 处理容器中需要代理的类型
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static ContainerBuilder BuildAopProxy(IServiceCollection service)
        {
            if (service is null)
                throw new ArgumentNullException(nameof(service));

            ContainerBuilder proxyServiceCollection = new ContainerBuilder();

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
                proxyServiceCollection.RegisterInstance(ServiceDescriptor.Describe(serviceType, newType, item.Lifetime));
            }
            return proxyServiceCollection;
        }

        /// <summary>
        /// 使用 CZGL.AOP 处理容器中需要代理的类型
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IContainer BuildAopProxy(this IContainer service)
        {
            if (service is null)
                throw new ArgumentNullException(nameof(service));

            ContainerBuilder proxyServiceCollection = new ContainerBuilder();
            IEnumerable<IComponentRegistration> serviceList = service.ComponentRegistry.Registrations;

            foreach (ComponentRegistration item in serviceList)
            {
                Type newType;
                Type serviceType = item.Services.OfType<IServiceWithType>().Select(x => x.ServiceType).FirstOrDefault();
                Type implementationType = item.Activator.LimitType;
                if (implementationType.GetCustomAttribute(typeof(InterceptorAttribute)) == null)
                {
                    proxyServiceCollection.RegisterComponent(item);
                    continue;
                }
                if (serviceType == implementationType)
                {
                    newType = DynamicProxy.CreateProxyClassType(implementationType);
                }
                else
                {
                    newType = DynamicProxy.CreateProxyClassType(serviceType, implementationType, false);
                }
                proxyServiceCollection.RegisterType(implementationType).As(serviceType).InstancePerMatchingLifetimeScope(item.MatchingLifetimeScopeTags());
            }
            return proxyServiceCollection.Build();
        }
    }
}
