using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace WcfProxyInterceptionTests
{
    public class WcfProxyInterceptor : IInterceptionBehavior
    {
        private readonly IUnityContainer _container;

        public WcfProxyInterceptor(IUnityContainer container)
        {
            _container = container;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            // create new instance of proxy (because previous call might have closed proxy)
            var proxy = _container.Resolve(input.MethodBase.DeclaringType, "Basic") as ICommunicationObject;

            // ensure that we got ICommunicationObject
            if (proxy == null)
                throw new InvalidOperationException("Interception attempted on non wcf proxy interface");

            var success = false;

            try
            {
                var args = input.Arguments.Cast<object>().ToArray();

                // invoke method on new proxy
                var result = input.MethodBase.Invoke(proxy, args);

                proxy.Close();
                success = true;

                return input.CreateMethodReturn(result, args);
            }
            catch (Exception ex)
            {
                if (!success)
                    proxy.Abort();

                return input.CreateExceptionMethodReturn(ex);
            }
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return new[] { typeof(ICommunicationObject) };
        }

        public bool WillExecute
        {
            get { return true; }
        }
    }
}
