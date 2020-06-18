using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AOP.Autofac
{
	/// <summary>
	/// Autofac 注入服务
	/// </summary>
	public class AOPServiceProxviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        AutofacServiceProviderFactory factory;
        public AOPServiceProxviderFactory(Action<ContainerBuilder> configurationAction = null)
        {
            factory = new AutofacServiceProviderFactory(configurationAction);
        }
        public ContainerBuilder CreateBuilder(IServiceCollection services)
		{
			return DependencyInjectionService.BuildAopProxy(services);
        }

        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            return factory.CreateServiceProvider(containerBuilder);
        }
    }
}
