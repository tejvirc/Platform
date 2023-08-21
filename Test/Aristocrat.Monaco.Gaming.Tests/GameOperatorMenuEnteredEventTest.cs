namespace Aristocrat.Monaco.Gaming.Tests
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for GameOperatorMenuEnteredEventTest and is intended
    ///     to contain all GameOperatorMenuEnteredEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class GameOperatorMenuEnteredEventTest
    {
        /// <summary>
        ///     A test for the parameterless constructor
        /// </summary>
        [TestMethod]
        public void ParameterlessConstructorTest()
        {
            var target = new GameOperatorMenuEnteredEvent();
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///     A test for serialization
        /// </summary>
        [TestMethod]
        public void SerializeTest()
        {
            var originalEvent = new GameOperatorMenuEnteredEvent();
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("GameOperatorMenuEnteredEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (GameOperatorMenuEnteredEvent)formatter.Deserialize(stream);

            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
        }
    }
}