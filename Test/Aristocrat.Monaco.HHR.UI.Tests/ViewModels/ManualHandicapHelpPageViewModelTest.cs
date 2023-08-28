namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Windows;
    using Accounting.Contracts;
    using Gaming.Contracts;
    using Hardware.Contracts.Button;
    using Kernel;
    using Test.Common;
    using UI.Menu;
    using UI.ViewModels;
    using Moq;

    [TestClass]
    public class ManualHandicapHelpPageViewModelTest
    {
        private ManualHandicapHelpPageViewModel _target;

        private Mock<IServiceManager> _serviceManager;
        private Mock<IPropertiesManager> _properties;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IBank> _bank;
        private Mock<IEventBus> _eventBus;

        private int _buttonId = -1;

        [TestInitialize]
        public void TestInitialization()
        {
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Publish(It.IsAny<DownEvent>())).Callback((DownEvent ev) => { _buttonId = ev.LogicalId; });

            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);

            if (Application.Current == null)
            {
                new Application();
            }

            _target = new ManualHandicapHelpPageViewModel(
                _eventBus.Object,
                _bank.Object,
                _gameProvider.Object,
                _properties.Object);

            _target.Init(Command.ManualHandicapHelp);

            // Reset the bet up/down delay counter so that we don't hit the limit while testing.
            HostPageViewModelManager.ResetBetButtonDelayLimit(
                DateTime.UtcNow.Ticks - HostPageViewModelManager.BetUpDownDelayTimeTicks - 1);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void VerifyCommands()
        {
            Assert.IsTrue(_target.Commands.Count(x => x.Command == Command.Bet) != 0);
            Assert.IsTrue(_target.Commands.Count(x => x.Command == Command.ManualHandicap) != 0);
            Assert.IsTrue(_target.Commands.Count(x => x.Command == Command.Help) != 0);
            Assert.IsTrue(_target.Commands.Count(x => x.Command == Command.ReturnToGame) != 0);
        }

        [TestMethod]
        public void VerifyRest()
        {
            _target.Reset();
            Assert.IsTrue(_target.Commands == null || _target.Commands.Count() == 0);
        }

        [TestMethod]
        public void ClickButton_BetUp_ExpectDownEventWithBetUP()
        {
            _target.Commands.First(c => c.Command == Command.Bet).Execute(Command.BetUp);
            Assert.AreEqual((int)ButtonLogicalId.BetUp, _buttonId);
        }

        [TestMethod]
        public void ClickButton_BetDown_ExpectDownEventWithBetDown()
        {
            _target.Commands.First(c => c.Command == Command.Bet).Execute(Command.BetDown);
            Assert.AreEqual((int)ButtonLogicalId.BetDown, _buttonId);
        }
    }
}
