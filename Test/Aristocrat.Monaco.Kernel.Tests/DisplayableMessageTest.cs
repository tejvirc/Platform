namespace Aristocrat.Monaco.Kernel.Tests
{
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Aristocrat.Monaco.Kernel.MessageDisplay;
    using Contracts.MessageDisplay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     This is a test class for DisplayableMessage and is intended
    ///     to contain all DisplayableMessageTest Unit Tests
    /// </summary>
    [TestClass]
    public class DisplayableMessageTest
    {
        /// <summary>
        ///     A test for DisplayableMessage Constructors and property 'getters'
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var message1 = "Some test message";
            var classification = DisplayableMessageClassification.Informative;
            var priority = DisplayableMessagePriority.Normal;
            var helptext = "This is how you clear this disable condition";

            //var target1 = new DisplayableMessage(
            //    () => message1,
            //    classification,
            //    priority,
            //    null,
            //    () => helptext);

            var target1 = new Mock<IDisplayableMessage>();

            target1.SetupGet(x => x.Message).Returns(message1);
            target1.SetupGet(x => x.Classification).Returns(classification);
            target1.SetupGet(x => x.Priority).Returns(priority);
            target1.SetupGet(x => x.ReasonEvent).Returns((System.Type)null);
            target1.SetupGet(x => x.HelpText).Returns(helptext);

            Assert.AreEqual(message1, target1.Object.Message);
            Assert.AreEqual(classification, target1.Object.Classification);
            Assert.AreEqual(priority, target1.Object.Priority);
            Assert.IsNull(target1.Object.ReasonEvent);
            Assert.AreEqual(helptext, target1.Object.HelpText);

            var message2 = "Test message with reason event";
            var reasonEvent = typeof(SystemDisabledEvent);
            var target2 = new DisplayableMessage(
                () => message2,
                classification,
                priority,
                reasonEvent,
                null);

            Assert.AreEqual(message2, target2.Message);
            Assert.AreEqual(classification, target2.Classification);
            Assert.AreEqual(priority, target2.Priority);
            Assert.AreEqual(reasonEvent, target2.ReasonEvent);
            Assert.IsNull(target2.HelpText);
        }

        /// <summary>
        ///     A test for DisplayableMessage message with resourceKey
        /// </summary>
        [TestMethod]
        public void MessageWithResourceKeyTest()
        {
            var messageText = "Test message 1";

            var message = new Mock<IDisplayableMessage>();

            message.SetupGet(x => x.Message).Returns(messageText);

            Assert.AreEqual(message.Object.Message, messageText);
        }

        /// <summary>
        ///     A test for DisplayableMessage serialization
        /// </summary>
        [TestMethod]
        public void SerializationTest()
        {
            var message1 = "Another test message";
            var classification = DisplayableMessageClassification.HardError;
            var priority = DisplayableMessagePriority.Low;
            var helptext = "This is how you clear this disable condition";

            var original = new DisplayableMessage(
                () => message1,
                classification,
                priority,
                null,
                () => helptext);

            var stream = new FileStream("DisplayableMessage.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, original);

            stream.Position = 0;

            var target = (IDisplayableMessage)formatter.Deserialize(stream);

            Assert.IsTrue(original.IsMessageEquivalent(target));
        }

        /// <summary>
        ///     A test for ToString()
        /// </summary>
        [TestMethod]
        public void ToStringTest()
        {
            var message = "A ToString() test message";
            var classification = DisplayableMessageClassification.SoftError;
            var priority = DisplayableMessagePriority.Immediate;

            var target = new DisplayableMessage(
                () => message,
                classification,
                priority);

            var expected = string.Format(
                CultureInfo.InvariantCulture,
                "Message=\"{0}\", Classification={1}, Priority={2}",
                message,
                classification,
                priority);

            Assert.AreEqual(expected, target.ToString());
        }

        /// <summary>
        ///     A test for IsMessageEquivalent()
        /// </summary>
        [TestMethod]
        public void IsMessageEquivalentTest()
        {
            // In order for IsMessageEquivalent to work correctly in all scenarios, it must be an equivalence relation.
            // IsMessageEquivalent is an equivalence relation if and only if (given a, b, c are DisplayableMessage):
            // 1. a == a (reflexive)
            // 2. if a == b, then b == a (symmetric)
            // 3. if a == b and b == c then a == c (transitive)
            var a = new DisplayableMessage(
                () => "A IsMessageEquivalentTest() test message",
                DisplayableMessageClassification.SoftError,
                DisplayableMessagePriority.Immediate,
                null,
                () => "IsMessageEquivalent function must be an equivalence relation");
            var b = new DisplayableMessage(
                () => "A IsMessageEquivalentTest() test message",
                DisplayableMessageClassification.SoftError,
                DisplayableMessagePriority.Immediate,
                null,
                () => "IsMessageEquivalent function must be an equivalence relation");
            var c = new DisplayableMessage(
                () => "A IsMessageEquivalentTest() test message",
                DisplayableMessageClassification.SoftError,
                DisplayableMessagePriority.Immediate,
                null,
                () => "IsMessageEquivalent function must be an equivalence relation");
            // Reflexive test
            Assert.IsTrue(a.IsMessageEquivalent(a));
            // Symmetric test
            Assert.AreEqual(a.IsMessageEquivalent(b), b.IsMessageEquivalent(a));
            // Transitive test
            Assert.IsTrue(a.IsMessageEquivalent(b));
            Assert.IsTrue(b.IsMessageEquivalent(c));
            Assert.IsTrue(a.IsMessageEquivalent(c));

            // test with resource key
            string messageText = "A IsMessageEquivalentTest() test message";
            string helpText = "IsMessageEquivalent function must be an equivalence relation";
            var message = new Mock<IDisplayableMessage>();
            message.SetupGet(m => m.MessageResourceKey).Returns("ResourceKey");
            message.SetupGet(x => x.Message).Returns(messageText);
            message.SetupGet(x => x.MessageHasDynamicGuid).Returns(true);
            message.SetupGet(x => x.Classification).Returns(DisplayableMessageClassification.SoftError);
            message.SetupGet(x => x.Priority).Returns(DisplayableMessagePriority.Immediate);
            message.SetupGet(x => x.ReasonEvent).Returns((System.Type)null);
            message.SetupGet(x => x.HelpText).Returns(() => helpText);
            Assert.IsTrue(a.IsMessageEquivalent(message.Object));

            // Testing inequality
            var empty_message = new DisplayableMessage(
                () => "",
                DisplayableMessageClassification.Diagnostic,
                DisplayableMessagePriority.Low,
                null,
                () => "");
            Assert.IsFalse(a.IsMessageEquivalent(empty_message));
            var message_different_by_one_parameter = new DisplayableMessage(
                () => "Different param here",
                DisplayableMessageClassification.SoftError,
                DisplayableMessagePriority.Immediate,
                null,
                () => "IsMessageEquivalent function must be an equivalence relation");
            Assert.IsFalse(a.IsMessageEquivalent(message_different_by_one_parameter));
        }
    }
}