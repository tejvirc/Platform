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
    public class LP02StartupHandlerTest
    {
        private LP02StartupHandler _target;
        private Mock<ISasDisableProvider> _disableProvider;
        private Mock<ISystemDisableManager> _systemDisableManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _disableProvider = new Mock<ISasDisableProvider>(MockBehavior.Default);
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);

            _target = new LP02StartupHandler(_disableProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.Startup));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullSasDisableManagerTest()
        {
            _target = new LP02StartupHandler(null);
        }

        [DataRow((byte)0, new[] { DisableState.DisabledByHost0, DisableState.PowerUpDisabledByHost0 }, false, DisplayName = "Enable for host 0")]
        [DataRow((byte)1, new[] { DisableState.DisabledByHost1, DisableState.PowerUpDisabledByHost1 }, false, DisplayName = "Enable for host 1")]
        [DataRow((byte)0, new[] { DisableState.DisabledByHost0, DisableState.PowerUpDisabledByHost0 }, true, DisplayName = "Enable for host 0")]
        [DataRow((byte)1, new[] { DisableState.DisabledByHost1, DisableState.PowerUpDisabledByHost1 }, true, DisplayName = "Enable for host 1")]
        [TestMethod]
        public void HandleTest(byte clientNumber, DisableState[] disableState, bool handlesGeneralControl)
        {
            _disableProvider.Setup(m => m.Enable(disableState)).Returns(Task.CompletedTask).Verifiable();

            var clientConfiguration = new SasClientConfiguration
            {
                ClientNumber = clientNumber,
                HandlesGeneralControl = handlesGeneralControl
            };

            var actual = _target.Handle(new LongPollSASClientConfigurationData(clientConfiguration));

            Assert.IsNull(actual);
            _disableProvider.Verify();
            _systemDisableManager.Verify();
        }
    }
}