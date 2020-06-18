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
        public static ContainerBuilder BuildAopProxy(IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            // 将 ASP.NET Core 中的依赖注入服务转到 Autofac 中
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(CreateBuildAopProxy(services));

            return containerBuilder;
        }

        /// <summary>
        /// 使用 CZGL.AOP 处理容器中需要代理的类型
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private static IServiceCollection CreateBuildAopProxy(this IServiceCollection service)
        {
            IServiceCollection proxyServiceCollection = new ServiceCollection();

            foreach (ServiceDescriptor item in service)
            {
                Type newType;
                Type serviceType = item.ServiceType;
                Type implementationType = item.ImplementationType;

                if (implementationType is null)
                {
                    if (item.ImplementationInstance != null)
                    {
                        switch (item.Lifetime)
                        {
                            case ServiceLifetime.Singleton:
                                proxyServiceCollection.AddSingleton(serviceType, item.ImplementationInstance);
                                break;
                            case ServiceLifetime.Scoped:
                                proxyServiceCollection.AddScoped(serviceType);
                                break;
                            case ServiceLifetime.Transient:
                                proxyServiceCollection.AddSingleton(serviceType, item.ImplementationInstance);
                                break;
                        }
                    }

                    else
                    {
                        switch (item.Lifetime)
                        {
                            case ServiceLifetime.Singleton:
                                proxyServiceCollection.AddSingleton(serviceType, item.ImplementationFactory);
                                break;
                            case ServiceLifetime.Scoped:
                                proxyServiceCollection.AddScoped(serviceType, item.ImplementationFactory);
                                break;
                            case ServiceLifetime.Transient:
                                proxyServiceCollection.AddTransient(serviceType, item.ImplementationFactory);
                                break;
                        }
                    }
                    continue;
                }

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
                if (implementationType?.GetCustomAttribute(typeof(InterceptorAttribute)) == null)
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
                proxyServiceCollection.RegisterComponent(item);
                proxyServiceCollection.RegisterType(newType).As(serviceType);
            }
            return proxyServiceCollection.Build();
        }
    }
}
