namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using System;
    using System.Reflection;
    using Accounting.Contracts.CoinAcceptor;
    using Hardware.Contracts.PWM;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CoinInCompletedEventTest
    {
        [TestMethod]
        public void PublicConstructorTest()
        {
            long amount = 100000;
            var target = new CoinInCompletedEvent(new Coin { Value = amount }, null);

            Assert.IsNotNull(target);
            Assert.AreEqual(amount, target.Coin.Value);
        }

        [TestMethod]
        public void PrivateConstructorTest()
        {
            Type t = typeof(CoinInCompletedEvent);
            Type[] paramTypes = { };
            ConstructorInfo ci = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                paramTypes,
                null);

            var target = (CoinInCompletedEvent)ci.Invoke(new object[] { });

            Assert.IsNotNull(target);
        }
    }
}
