namespace Aristocrat.Monaco.Application.Tests
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test for SystemEnabledByOperatorEventTest and contains
    ///     all the unit tests for that event.
    /// </summary>
    [TestClass]
    public class SystemEnabledByOperatorEventTest
    {
        /// <summary>
        ///     A test for the parameterless constructor
        /// </summary>
        [TestMethod]
        public void ParameterlessConstructorTest()
        {
            var theEvent = new SystemEnabledByOperatorEvent();
            Assert.IsNotNull(theEvent);
        }

        /// <summary>
        ///     A test for serialization
        /// </summary>
        [TestMethod]
        public void SerializeTest()
        {
            var originalEvent = new SystemEnabledByOperatorEvent();
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("SystemEnabledByOperatorEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (SystemEnabledByOperatorEvent)formatter.Deserialize(stream);

            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
        }
    }
}