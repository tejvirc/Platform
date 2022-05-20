namespace Aristocrat.Monaco.Sas.Tests.Base
{
    using System.Collections.Generic;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Base;
    using Test.Common;

    [TestClass]
    public class SasNoteAcceptorProviderTests
    {
        //private Mock<INoteAcceptor> _noteAcceptor;
        //private SasNoteAcceptorProvider _target;
        /*
        private static IEnumerable<object[]> SuccessfulConfigureBillDenominations => new List<object[]>
        {
            new object[]
            {
                new List<ulong>
                {
                    1, 10_000
                },
                NoteAcceptorDenominations.Hundred | NoteAcceptorDenominations.Voucher
            },
            new object[]
            {
                new List<ulong>
                {
                    1, 100_000
                },
                NoteAcceptorDenominations.Voucher
            },
            new object[]
            {
                new List<ulong>
                {
                    100, 200, 500, 1_000, 2_000, 2_500, 5_000, 10_000
                },
                NoteAcceptorDenominations.All
            }
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _noteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Default);
            _target = new SasNoteAcceptorProvider();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void EnableBillAcceptorTest()
        {
            _noteAcceptor.Setup(x => x.Enable(It.Is<EnabledReasons>(reason => reason == EnabledReasons.Backend)));
            _target.EnableBillAcceptor();
            _noteAcceptor.VerifyAll();
        }

        [TestMethod]
        public void DisableBillAcceptorTest()
        {
            _noteAcceptor.Setup(x => x.Disable(It.Is<DisabledReasons>(reason => reason == DisabledReasons.Backend)));
            _target.DisableBillAcceptor();
            _noteAcceptor.VerifyAll();
        }

        [DynamicData(nameof(SuccessfulConfigureBillDenominations))]
        [DataTestMethod]
        public void SuccessfulConfigureBillDenominationsTest(IEnumerable<ulong> denomList, NoteAcceptorDenominations expectedDenoms)
        {
            _noteAcceptor.SetupGet(m => m.Denominations).Returns(NoteAcceptorDenominations.All);
            _noteAcceptor.SetupSet(m => m.Denominations = It.Is<NoteAcceptorDenominations>(denoms => denoms == expectedDenoms));
            Assert.IsTrue(_target.ConfigureBillDenominations(denomList));

            _noteAcceptor.VerifyAll();
        }

        [TestMethod]
        public void FailedConfigureBillDenominationsTest()
        {
            var denomList = new List<ulong>
            {
                1, 10_000
            };

            MoqServiceManager.RemoveService<INoteAcceptor>();
            Assert.IsFalse(_target.ConfigureBillDenominations(denomList));

            _noteAcceptor.VerifyAll();
        }*/
    }
}