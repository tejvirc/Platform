namespace Aristocrat.Monaco.Hardware.Tests.ContractsTests
{
    using Contracts.NoteAcceptor;
    using Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This file contains the tests for NoteAcceptorContract
    /// </summary>
    [TestClass]
    public class NoteAcceptorContractTests
    {
        [TestMethod]
        public void EnabledEventTest()
        {
            // constructor with EnabledReasons test
            var target = new EnabledEvent(EnabledReasons.Backend);
            Assert.IsNotNull(target);
            Assert.AreEqual(EnabledReasons.Backend, target.Reasons);
        }

        [TestMethod]
        public void DisabledEventTest()
        {
            // constructor with DisabledReasons test
            var target = new DisabledEvent(DisabledReasons.Backend);
            Assert.IsNotNull(target);
            Assert.AreEqual(DisabledReasons.Backend, target.Reasons);
        }

        [TestMethod]
        public void RebootNotStackedEventTest()
        {
            // constructor test
            long amount = 100;
            var target = new RebootNotStackedEvent(true, amount);
            Assert.IsNotNull(target);
            Assert.AreEqual(amount, target.Amount);
            Assert.IsTrue(target.IsVoucher);
        }

        [TestMethod]
        public void VoucherEscrowedEventTest()
        {
            // no parameter constructor tests
            var target = new VoucherEscrowedEvent("");
            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.Barcode);

            // constructor with barcode test
            string barcode = "1234";
            target = new VoucherEscrowedEvent(barcode);
            Assert.IsNotNull(target);
            Assert.AreEqual(barcode, target.Barcode);
        }

        [TestMethod]
        public void CurrencyEscrowedEventTest()
        {
            // no parameter constructor tests
            var target = new CurrencyEscrowedEvent(new Note());
            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.Note.Value);
            Assert.AreEqual(ISOCurrencyCode.USD, target.Note.CurrencyCode);

            // constructor with denomination test
            const int denomination = 10;
            target = new CurrencyEscrowedEvent(new Note{Value = denomination});
            Assert.IsNotNull(target);
            Assert.AreEqual(denomination, target.Note.Value);
            Assert.AreEqual(ISOCurrencyCode.USD, target.Note.CurrencyCode);

            const ISOCurrencyCode iso = ISOCurrencyCode.AUD;
            target = new CurrencyEscrowedEvent(new Note{ISOCurrencySymbol = "AUD", Value = denomination});
            Assert.IsNotNull(target);
            Assert.AreEqual(denomination, target.Note.Value);
            Assert.AreEqual(iso, target.Note.CurrencyCode);
        }

        [TestMethod]
        public void DocumentRejectedEventTest()
        {
            // no parameter constructor tests
            var target = new DocumentRejectedEvent();
            Assert.IsNotNull(target);

            // constructor with note acceptor ID
            int noteAcceptorId = 5;
            target = new DocumentRejectedEvent(noteAcceptorId);
            Assert.IsNotNull(target);
            Assert.AreEqual(noteAcceptorId, target.NoteAcceptorId);
        }

        [TestMethod]
        public void CurrencyStackedEventTest()
        {
            // no parameter constructor tests
            var target = new CurrencyStackedEvent(new Note());
            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.Note.Value);

            // constructor with value test
            const int value = 123;
            target = new CurrencyStackedEvent(new Note{Value = value});
            Assert.IsNotNull(target);
            Assert.AreEqual(value, target.Note.Value);
        }

        [TestMethod]
        public void HardwareFaultClearEventTest()
        {
            NoteAcceptorFaultTypes error = NoteAcceptorFaultTypes.MechanicalFault;
            var target = new HardwareFaultClearEvent(error);
            Assert.IsNotNull(target);
            Assert.AreEqual(error, target.Fault);
        }

        [TestMethod]
        public void HardwareFaultEventTest()
        {
            NoteAcceptorFaultTypes error = NoteAcceptorFaultTypes.MechanicalFault;
            var target = new HardwareFaultEvent(error);
            Assert.IsNotNull(target);
            Assert.AreEqual(error, target.Fault);
        }
    }
}
