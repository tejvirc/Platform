namespace Aristocrat.Monaco.Hardware.Tests.HardMeter
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts.HardMeter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for OnEventTest and is intended
    ///     to contain all OnEventTest Unit Tests.
    /// </summary>
    [TestClass]
    public class OnEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>A test for <c>OnEvent</c> Constructor.</summary>
        [TestMethod]
        public void OnEventConstructorTest()
        {
            var target = new OnEvent();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(OnEvent));
        }

        /// <summary>A test for <c>OnEvent</c> 1 Parameter Constructor.</summary>
        [TestMethod]
        public void OnEvent1ParameterConstructorTest()
        {
            const int LogicalId = 1;
            var target = new OnEvent(LogicalId);
            Assert.AreEqual(LogicalId, target.LogicalId);
        }

        /// <summary>A test for <c>OnEvent</c> serialization.</summary>
        [TestMethod]
        public void OnEventSerializeTest()
        {
            var originalEvent = new OnEvent();
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("InspectedEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (OnEvent)formatter.Deserialize(stream);

            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
        }
    }
}
