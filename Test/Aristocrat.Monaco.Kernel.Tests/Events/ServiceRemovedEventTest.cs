namespace Aristocrat.Monaco.Kernel.Tests
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for ServiceRemovedEventTest and is intended
    ///     to contain all ServiceRemovedEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class ServiceRemovedEventTest
    {
        /// <summary>
        ///     A test for setting and getting the ServiceType property
        /// </summary>
        [TestMethod]
        public void ServiceTypeTest()
        {
            var expected = typeof(IServiceManager);
            var target = new ServiceRemovedEvent(expected);
            var actual = target.ServiceType;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for the parameterless constructor
        /// </summary>
        [TestMethod]
        public void ParameterlessConstructorTest()
        {
            var target = new ServiceRemovedEvent();
            Assert.IsNull(target.ServiceType);
        }

        /// <summary>
        ///     A test for the 1-parameter constructor
        /// </summary>
        [TestMethod]
        public void ParameterConstructorTest()
        {
            var expected = typeof(IServiceManager);
            var target = new ServiceRemovedEvent(expected);
            Assert.AreEqual(expected, target.ServiceType);
        }

        /// <summary>
        ///     A test for ToString()
        /// </summary>
        [TestMethod]
        public void ToStringTest()
        {
            var serviceType = typeof(IServiceManager);
            var target = new ServiceRemovedEvent(serviceType);
            var expected =
                "Aristocrat.Monaco.Kernel.ServiceRemovedEvent [GUID=" +
                target.GloballyUniqueId +
                ", ServiceType=" +
                serviceType +
                "]";
            Assert.AreEqual(expected, target.ToString());
        }
    }
}