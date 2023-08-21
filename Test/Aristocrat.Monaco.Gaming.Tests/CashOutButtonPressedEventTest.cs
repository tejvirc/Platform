namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;

    [TestClass]
    public class CashOutButtonPressedEventTest
    {
        [TestMethod]
        public void ParameterlessConstructorTest()
        {
            var target = new CashOutButtonPressedEvent();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void SerializeTest()
        {
            AssertEx.IsAttributeDefined(typeof(CashOutButtonPressedEvent), typeof(SerializableAttribute));
        }
    }
}