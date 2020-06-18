using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CZGL.AOP
{

    public class ProxyTypeBuilder
    {
        private Dictionary<string, Type> _actionAttributes = new Dictionary<string, Type>();
        private NoActionAttributeModel actions = new NoActionAttributeModel();
        public ProxyTypeBuilder() { }
        public ProxyTypeBuilder(Type[] attributes)
        {
            foreach (var item in attributes)
            {
                if (_actionAttributes.ContainsKey(item.Name))
                    continue;
                if (item.BaseType != typeof(ActionAttribute))
                    throw new Exception($"{item.Name} 不继承 {nameof(ActionAttribute)}");
                else _actionAttributes.Add(item.Name, item);
            }
        }

        /// <summary>
        /// 拦截一个方法
        /// </summary>
        /// <param name="action">继承了 ActionAttribute 的拦截器</param>
        /// <param name="info">要代理的方法 MethodInfo </param>
        /// <returns></returns>
        public ProxyTypeBuilder AddProxyMethod(Type action, MethodInfo info)
        {
            if (action == null || info == null)
                throw new ArgumentNullException($"{(action == null ? nameof(action) : nameof(info))} 不能为 null ");
            if (action.BaseType != typeof(ActionAttribute))
                throw new Exception($"{action.Name} 不继承 {nameof(ActionAttribute)}");

            if (!_actionAttributes.ContainsKey(action.Name))
                _actionAttributes.Add(action.Name, action);

            actions.MethodNames.Add(info,action.Name);

            return this;
        }

        /// <summary>
        /// 拦截一个方法
        /// </summary>
        /// <param name="action">继承了 ActionAttribute 的拦截器</param>
        /// <param name="info">要代理的方法 MethodInfo </param>
        /// <returns></returns>
        public ProxyTypeBuilder AddProxyMethod(string actionName, MethodInfo info)
        {
            if (!_actionAttributes.ContainsKey(actionName))
                throw new ArgumentNullException($"无法找到名为 {actionName} 的拦截器。");

            actions.MethodNames.Add(info,actionName);

            return this;
        }

        /// <summary>
        /// 拦截一个属性
        /// </summary>
        /// <param name="action">继承了 ActionAttribute 的拦截器</param>
        /// <param name="info">要代理的方法 MethodInfo </param>
        /// <returns></returns>
        public ProxyTypeBuilder AddProxyProperty(Type action, PropertyInfo info)
        {
            if (action == null || info == null)
                throw new ArgumentNullException($"{(action == null ? nameof(action) : nameof(info))} 不能为 null ");
            if (action.BaseType != typeof(ActionAttribute))
                throw new Exception($"{action.Name} 不继承 {nameof(ActionAttribute)}");
            if (!_actionAttributes.ContainsKey(action.Name))
                _actionAttributes.Add(action.Name, action);

            actions.PropertyNames.Add( info, action.Name);

            return this;
        }

        /// <summary>
        /// 拦截一个属性
        /// </summary>
        /// <param name="action">继承了 ActionAttribute 的拦截器</param>
        /// <param name="info">要代理的方法 MethodInfo </param>
        /// <returns></returns>
        public ProxyTypeBuilder AddProxyProperty(string actionName, PropertyInfo info)
        {
            if (!_actionAttributes.ContainsKey(actionName))
                throw new ArgumentNullException($"无法找到名为 {actionName} 的拦截器。");

            actions.PropertyNames.Add(info, actionName);

            return this;
        }


        internal NoActionAttributeModel Build()
        {
            actions.Actions = _actionAttributes;
            return actions;
        }
    }

    /// <summary>
    /// 用于传递非侵入式代理时的参数
    /// </summary>
    internal class NoActionAttributeModel
    {
        internal Dictionary<string, Type> Actions { get; set; }
        internal Dictionary<MethodInfo, string> MethodNames { get; set; } = new Dictionary<MethodInfo, string>();
        internal Dictionary<PropertyInfo, string> PropertyNames { get; set; } = new Dictionary<PropertyInfo, string>();
    }

}
