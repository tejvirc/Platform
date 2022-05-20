namespace Aristocrat.Monaco.Hardware.Tests.HardMeter
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts.HardMeter;
    using Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for EnabledEventTest and is intended
    ///     to contain all EnabledEventTest Unit Tests.
    /// </summary>
    [TestClass]
    public class EnabledEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>A test for setting and getting the Reasons property.</summary>
        [TestMethod]
        public void EnabledEventReasonsTest()
        {
            const EnabledReasons Reasons = EnabledReasons.Service;
            var target = new EnabledEvent(Reasons);
            Assert.AreEqual(Reasons, target.Reasons);
        }

        /// <summary>A test for <c>EnabledEvent</c> 1 Parameter Constructor.</summary>
        [TestMethod]
        public void EnabledEvent1ParameterConstructorTest()
        {
            const EnabledReasons Reasons = EnabledReasons.Service;
            var target = new EnabledEvent(Reasons);
            Assert.AreEqual(Reasons, target.Reasons);
        }

        /// <summary>A test for <c>EnabledEvent</c> serialization.</summary>
        [TestMethod]
        public void EnabledEventSerializeTest()
        {
            const EnabledReasons Reasons = EnabledReasons.Service;
            var originalEvent = new EnabledEvent(Reasons);
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("EnabledEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (EnabledEvent)formatter.Deserialize(stream);

            Assert.AreEqual(Reasons, target.Reasons);
            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
        }
    }
}
