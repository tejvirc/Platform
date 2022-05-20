namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using System;
    using System.Reflection;
    using Hardware.Contracts.NoteAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CurrencyInCompletedEventTest
    {
        [TestMethod]
        public void PublicConstructorTest()
        {
            long amount = 1000;
            var target = new CurrencyInCompletedEvent(amount);

            Assert.IsNotNull(target);
            Assert.AreEqual(amount, target.Amount);
            Assert.IsNull(target.Note);
        }

        [TestMethod]
        public void PublicConstructorWithNoteTest()
        {
            long amount = 1000;
            var target = new CurrencyInCompletedEvent(amount, new Note { Value = (int)amount });

            Assert.IsNotNull(target);
            Assert.AreEqual(amount, target.Amount);
            Assert.AreEqual((int)amount, target.Note.Value);
        }

        [TestMethod]
        public void PrivateConstructorTest()
        {
            Type t = typeof(CurrencyInCompletedEvent);
            Type[] paramTypes = { };
            ConstructorInfo ci = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                paramTypes,
                null);

            var target = (CurrencyInCompletedEvent)ci.Invoke(new object[] { });

            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.Amount);
        }
    }
}