namespace Aristocrat.Monaco.Hardware.Tests.HardMeter
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts.HardMeter;
    using Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for DisabledEventTest and is intended
    ///     to contain all DisabledEventTest Unit Tests.
    /// </summary>
    [TestClass]
    public class DisabledEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>A test for setting and getting the Reasons property.</summary>
        [TestMethod]
        public void DisabledEventReasonsTest()
        {
            const DisabledReasons Reasons = DisabledReasons.Service;
            var target = new DisabledEvent(Reasons);
            Assert.AreEqual(Reasons, target.Reasons);
        }

        /// <summary>A test for <c>DisabledEvent</c> 1 Parameter Constructor.</summary>
        [TestMethod]
        public void DisabledEvent1ParameterConstructorTest()
        {
            const DisabledReasons Reasons = DisabledReasons.Service;
            var target = new DisabledEvent(Reasons);
            Assert.AreEqual(Reasons, target.Reasons);
        }

        /// <summary>A test for <c>DisabledEvent</c> serialization.</summary>
        [TestMethod]
        public void DisabledEventSerializeTest()
        {
            const DisabledReasons Reasons = DisabledReasons.Service;
            var originalEvent = new DisabledEvent(Reasons);
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("DisabledEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (DisabledEvent)formatter.Deserialize(stream);

            Assert.AreEqual(Reasons, target.Reasons);
            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
        }
    }
}
