using System;
using System.Collections.Generic;
using System.Linq;
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
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), parameters?.Select(x => x.GetType()).ToArray());
            return DynamicProxy.CreateInterceptor<TInterface, TType>(parameters, false);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object[] parameters = null)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), parameters?.Select(x => x.GetType()).ToArray());
            return DynamicProxy.CreateInterceptor<TType, TType>(parameters, true);
        }

        #region

        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object t1)
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType() });
            return DynamicProxy.CreateInterceptor<TInterface, TType>(new object[] { t1 }, false);
        }

        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object t1, object t2)
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType() });
            return DynamicProxy.CreateInterceptor<TInterface, TType>(new object[] { t1, t2 }, false);
        }

        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object t1, object t2, object t3)
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType() });
            return DynamicProxy.CreateInterceptor<TInterface, TType>(new object[] { t1, t2, t3 }, false);
        }

        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object t1, object t2, object t3, object t4)
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType() });
            return DynamicProxy.CreateInterceptor<TInterface, TType>(new object[] { t1, t2, t3, t4 }, false);
        }

        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object t1, object t2, object t3, object t4, object t5)
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType(), t5.GetType() });
            return DynamicProxy.CreateInterceptor<TInterface, TType>(new object[] { t1, t2, t3, t4, t5 }, false);
        }

        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object t1, object t2, object t3, object t4, object t5, object t6)
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType(), t5.GetType(), t6.GetType() });
            return DynamicProxy.CreateInterceptor<TInterface, TType>(new object[] { t1, t2, t3, t4, t5, t6 }, false);
        }

        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object t1, object t2, object t3, object t4, object t5, object t6, object t7)
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType(), t5.GetType(), t6.GetType(), t7.GetType() });
            return DynamicProxy.CreateInterceptor<TInterface, TType>(new object[] { t1, t2, t3, t4, t5, t6, t7 }, false);
        }

        /// <summary>
        /// 从接口生成代理类
        /// </summary>
        /// <typeparam name="TInterface">接口</typeparam>
        /// <typeparam name="TType">实现的类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns></returns>
        public static TInterface CreateProxyOfInterface<TInterface, TType>(object t1, object t2, object t3, object t4, object t5, object t6, object t7, object t8)
            where TInterface : class
            where TType : TInterface
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType(), t5.GetType(), t6.GetType(), t7.GetType(), t8.GetType() });
            return DynamicProxy.CreateInterceptor<TInterface, TType>(new object[] { t1, t2, t3, t4, t5, t6, t7, t8 }, false);
        }



        #endregion


        #region 

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object t1)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType() });
            return DynamicProxy.CreateInterceptor<TType, TType>(new object[] { t1 }, true);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object t1, object t2)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType() });
            return DynamicProxy.CreateInterceptor<TType, TType>(new object[] { t1, t2 }, true);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object t1, object t2, object t3)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType() });
            return DynamicProxy.CreateInterceptor<TType, TType>(new object[] { t1, t2, t3 }, true);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object t1, object t2, object t3, object t4)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType() });
            return DynamicProxy.CreateInterceptor<TType, TType>(new object[] { t1, t2, t3, t4 }, true);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object t1, object t2, object t3, object t4, object t5)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType(), t5.GetType() });
            return DynamicProxy.CreateInterceptor<TType, TType>(new object[] { t1, t2, t3, t4, t5 }, true);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object t1, object t2, object t3, object t4, object t5, object t6)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType(), t5.GetType(), t6.GetType() });
            return DynamicProxy.CreateInterceptor<TType, TType>(new object[] { t1, t2, t3, t4, t5, t6 }, true);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object t1, object t2, object t3, object t4, object t5, object t6, object t7)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType(), t5.GetType(), t6.GetType(), t7.GetType() });
            return DynamicProxy.CreateInterceptor<TType, TType>(new object[] { t1, t2, t3, t4, t5, t6, t7 }, true);
        }

        /// <summary>
        /// 生成代理类型
        /// </summary>
        /// <typeparam name="TType">代理类型</typeparam>
        /// <param name="parameters">构造函数的参数</param>
        /// <returns>生成代理类</returns>
        public static TType CreateProxyOfClass<TType>(object t1, object t2, object t3, object t4, object t5, object t6, object t7, object t8)
            where TType : class
        {
            ThrowHasConstructor(typeof(TType), new Type[] { t1.GetType(), t2.GetType(), t3.GetType(), t4.GetType(), t5.GetType(), t6.GetType(), t7.GetType(), t8.GetType() });
            return DynamicProxy.CreateInterceptor<TType, TType>(new object[] { t1, t2, t3, t4, t5, t6, t7, t8 }, true);
        }

        #endregion

        /// <summary>
        /// 检查是否有相应的构造函数
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="types">传递的构造函数的参数</param>
        private static void ThrowHasConstructor(Type type, Type[] types = null)
        {
            if (type.GetConstructor(types == null ? new Type[0] { } : types) == null)
            {
                throw new System.MissingMethodException($"{type.Name} 类型中未找到与传递的参数列表相符合的构造函数！请检查参数个数或参数类型！");
            }
        }
    }
}
