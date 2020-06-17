using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AOP.MEDI
{
    /// <summary>
    /// ASP.NET Core 注入服务
    /// </summary>
    public class AOPServiceProxviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services.BuildAopProxy();
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return containerBuilder.BuildServiceProvider();
        }
    }
}
