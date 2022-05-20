namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GameDiagnosticsStartedEventTest
    {
        [TestMethod]
        public void ParameterConstructorTest()
        {
            var context = new Mock<IDiagnosticContext>();

            var target = new GameDiagnosticsStartedEvent(1, 2, "test", context.Object);

            Assert.IsNotNull(target);
            Assert.AreEqual(1, target.GameId);
            Assert.AreEqual(2, target.Denomination);
            Assert.AreEqual(context.Object, target.Context);
            Assert.AreEqual("test", target.Label);
        }

        [TestMethod]
        public void SerializeTest()
        {
            AssertEx.IsAttributeDefined(typeof(GameDiagnosticsStartedEvent), typeof(SerializableAttribute));
        }
    }
}