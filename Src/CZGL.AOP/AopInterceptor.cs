using System;
using System.Collections.Generic;
using System.Text;

namespace CZGL.AOP
{
    public class AopInterceptor
    {
        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object[] parameters = null)
            where TInterface : class, new()
            where TType : TInterface
        {
            return DynamicProxy.CreateInterceptor<TInterface,TType>(parameters,false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object[] parameters = null)
            where TType : class
        {
            return DynamicProxy.CreateInterceptor<TType, TType>(parameters, true);
        }
    }
}
