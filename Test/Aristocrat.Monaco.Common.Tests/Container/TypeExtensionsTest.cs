namespace Aristocrat.Monaco.Common.Tests.Container
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Common.Container;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TypeExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenIsImplementationOfThisTypeIsNullExpectException()
        {
            var type = (Type)null;
            type.IsImplementationOf(GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenIsImplementationOfInterfaceThisTypeIsNullExpectException()
        {
            var type = GetType();
            type.IsImplementationOf(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenIsImplementationOfInterfaceInterfaceTypeIsNullExpectException()
        {
            var type = typeof(Assembly);
            type.IsImplementationOf((Type)null);
        }

        [TestMethod]
        public void WhenIsImplementationOfInterfaceExpectTrue()
        {
            var type = typeof(List<>);
            var result = type.IsImplementationOf(typeof(IEnumerable<>));

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenIsImplementationOfInterfaceExpectFalse()
        {
            var type = typeof(List<>);
            var result = type.IsImplementationOf(typeof(IQueryable<>));

            Assert.AreEqual(false, result);
        }
    }
}