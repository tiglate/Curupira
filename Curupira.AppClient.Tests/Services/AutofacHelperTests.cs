using Autofac;
using Curupira.AppClient.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Curupira.AppClient.Tests.Services
{
    [TestClass]
    public class AutofacHelperTests
    {
        private IContainer _container;
        private AutofacHelper _autofacHelper;

        [TestInitialize]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            // Register types here
            builder.RegisterType<SomeImplementation>().As<ISomeInterface>().Named<ISomeInterface>("SomeName");
            _container = builder.Build();

            _autofacHelper = new AutofacHelper(_container);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _container.Dispose();
        }

        [TestMethod]
        public void GetNamedImplementationsOfInterface_ShouldReturnCorrectImplementations()
        {
            // Act
            var implementations = _autofacHelper.GetNamedImplementationsOfInterface<ISomeInterface>();

            // Assert
            Assert.AreEqual(1, implementations.Count);
            Assert.AreEqual("SomeName", implementations[0].Name);
            Assert.AreEqual(typeof(SomeImplementation), implementations[0].ImplementationType);
        }

        public interface ISomeInterface { }

        public class SomeImplementation : ISomeInterface { }
    }
}
