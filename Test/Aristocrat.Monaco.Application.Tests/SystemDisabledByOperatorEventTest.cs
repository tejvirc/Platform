namespace Aristocrat.Monaco.Application.Tests
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test for SystemDisabledByOperatorEventTest and contains
    ///     all the unit tests for that event.
    /// </summary>
    [TestClass]
    public class SystemDisabledByOperatorEventTest
    {
        /// <summary>
        ///     A test for the parameterless constructor
        /// </summary>
        [TestMethod]
        public void ParameterlessConstructorTest()
        {
            var theEvent = new SystemDisabledByOperatorEvent();
            Assert.IsNotNull(theEvent);
        }

        /// <summary>
        ///     A test for serialization
        /// </summary>
        [TestMethod]
        public void SerializeTest()
        {
            var originalEvent = new SystemDisabledByOperatorEvent();
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("SystemDisabledByOperatorEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (SystemDisabledByOperatorEvent)formatter.Deserialize(stream);

            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
        }
    }
}