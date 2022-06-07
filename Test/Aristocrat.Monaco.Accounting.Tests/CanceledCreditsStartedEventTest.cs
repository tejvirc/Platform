namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using Contracts.Handpay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;

    /// <summary>
    ///     This is a test class for CanceledCreditsStartedEventTest and is intended
    ///     to contain all CanceledCreditsStartedEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class CanceledCreditsStartedEventTest
    {
        /// <summary>
        ///     A test for Amount
        /// </summary>
        [TestMethod]
        public void ConstuctorTest()
        {
            var accessor = new HandpayStartedEvent(HandpayType.CancelCredit, 1000, 0, 0, 123, false);

            Assert.AreEqual(1000, accessor.CashableAmount);
        }

        /// <summary>
        ///     A test for CanceledCreditsStartedEvent serialization / deserialization
        /// </summary>
        [TestMethod]
        public void CanceledCreditsStartedEventSerializationTest()
        {
            AssertEx.IsAttributeDefined(typeof(HandpayStartedEvent), typeof(SerializableAttribute));
        }
    }
}
