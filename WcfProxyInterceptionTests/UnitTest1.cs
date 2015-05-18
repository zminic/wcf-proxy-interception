using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WcfProxyInterceptionTests.SampleService;

namespace WcfProxyInterceptionTests
{
    [TestClass]
    public class UnitTest1
    {
        private readonly IUnityContainer _container;

        public UnitTest1()
        {
            _container = new UnityContainer();

            _container.AddNewExtension<Interception>();

            _container.RegisterType<IService1, Service1Client>("Basic", new InjectionConstructor());

            _container.RegisterType<IService1, Service1Client>(new InjectionConstructor(),
                new Interceptor<InterfaceInterceptor>(),
                new InterceptionBehavior<WcfProxyInterceptor>());
        }

        [TestMethod]
        public void WcfInterceptor_CanCreateNewProxyIfPreviousIsClosed()
        {
            var service = _container.Resolve<IService1>();

            service.GetData(1);
            service.GetData(2);
        }

        [TestMethod]
        public async Task WcfInterceptor_CanCloseProxyForAsyncMethod()
        {
            var service = _container.Resolve<IService1>();

            var result = await service.GetIntAsync();

            Assert.AreEqual(result, 5);
        }

        [TestMethod]
        public async Task WcfInterceptor_CanCloseProxyForAsyncMethodThatReturnsError()
        {
            var service = _container.Resolve<IService1>();

            try
            {
                var result = await service.GetIntExceptionAsync();
            }
            catch
            {
                // ignored
            }
        }
    }
}
