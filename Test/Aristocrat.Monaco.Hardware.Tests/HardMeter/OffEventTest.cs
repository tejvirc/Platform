namespace Aristocrat.Monaco.Hardware.Tests.HardMeter
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts.HardMeter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for OffEventTest and is intended
    ///     to contain all OffEventTest Unit Tests.
    /// </summary>
    [TestClass]
    public class OffEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>A test for <c>OffEvent</c> Constructor.</summary>
        [TestMethod]
        public void OffEventConstructorTest()
        {
            var target = new OffEvent();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(OffEvent));
        }

        /// <summary>A test for <c>OffEvent</c> 1 Parameter Constructor.</summary>
        [TestMethod]
        public void OffEvent1ParameterConstructorTest()
        {
            const int LogicalId = 1;
            var target = new OffEvent(LogicalId);
            Assert.AreEqual(LogicalId, target.LogicalId);
        }

        /// <summary>A test for <c>OffEvent</c> serialization.</summary>
        [TestMethod]
        public void OffEventSerializeTest()
        {
            var originalEvent = new OffEvent();
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("OffEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (OffEvent)formatter.Deserialize(stream);

            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
        }
    }
}
