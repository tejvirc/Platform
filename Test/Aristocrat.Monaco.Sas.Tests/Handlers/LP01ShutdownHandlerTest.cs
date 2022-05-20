namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP01ShutdownHandlerTest
    {
        private LP01ShutdownHandler _target;
        private Mock<ISasDisableProvider> _disableProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _disableProvider = new Mock<ISasDisableProvider>(MockBehavior.Default);

            _target = new LP01ShutdownHandler(_disableProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.Shutdown));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullSasDisableManagerTest()
        {
            _target = new LP01ShutdownHandler(null);
        }

        [DataRow((byte)0, DisableState.DisabledByHost0, DisplayName = "Disable for host 0")]
        [DataRow((byte)1, DisableState.DisabledByHost1, DisplayName = "Disable for host 1")]
        [DataTestMethod]
        public void HandleTest(byte clientNumber, DisableState disableState)
        {
            _disableProvider.Setup(m => m.Disable(SystemDisablePriority.Normal, disableState))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var actual = _target.Handle(new LongPollSingleValueData<byte>(clientNumber));

            Assert.IsNull(actual);
            _disableProvider.Verify();
        }
    }
}
