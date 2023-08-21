namespace Aristocrat.Monaco.Common.Tests.Container
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using Common.Container;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector;

    [TestClass]
    public class ContainerExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRegisterManyForOpenGenericContainerIsNullExpectException()
        {
            var container = (Container)null;
            container.RegisterManyForOpenGeneric(typeof(IEnumerable<>), true, Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRegisterManyForOpenGenericServiceTypeIsNullExpectException()
        {
            var container = new Container();
            container.RegisterManyForOpenGeneric(null, true, Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRegisterManyForOpenGenericAssembliesIsNullExpectException()
        {
            var container = new Container();
            container.RegisterManyForOpenGeneric(typeof(IEnumerable<>), true, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenRegisterManyForOpenGenericServiceTypeNotContainsGenericExpectException()
        {
            var container = new Container();
            container.RegisterManyForOpenGeneric(typeof(IEnumerable), true, Assembly.GetExecutingAssembly());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRegisterContainerIsNullExpectException()
        {
            var container = (Container)null;
            container.Register("namespace", Assembly.GetExecutingAssembly(), Lifestyle.Singleton);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRegisterNamespaceIsNullExpectException()
        {
            var container = new Container();
            container.Register(null, Assembly.GetExecutingAssembly(), Lifestyle.Singleton);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRegisterAssemblyIsNullExpectException()
        {
            var container = new Container();
            container.Register("namespace", null, Lifestyle.Singleton);
        }

        [TestMethod]
        public void WhenRegisterWithValidParamsExpectSuccess()
        {
            var container = new Container();
            container.Register("namespace", Assembly.GetExecutingAssembly(), Lifestyle.Singleton);
        }

        [TestMethod]
        public void WhenRegisterManyForOpenGenericWithValidParamsExpectSuccess()
        {
            var container = new Container();
            container.RegisterManyForOpenGeneric(typeof(IEnumerable<>), true, Assembly.GetExecutingAssembly());
        }
    }
}