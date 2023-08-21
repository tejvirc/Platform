namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using Hardware.Contracts.Button;
    using Hhr.UI.Menu;
    using Hhr.UI.ViewModels;
    using Kernel;
    using Test.Common;
    using Moq;

    [TestClass]
    public class HelpPageViewModelTest
    {
        private HelpPageViewModel _target;
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IEventBus> _eventBus;
        private int _buttonId = -1;

        [TestInitialize]
        public void TestInitialization()
        {
            _serviceManagerMock = MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Publish(It.IsAny<DownEvent>())).Callback((DownEvent ev) => { _buttonId = ev.LogicalId; });

            _target = new HelpPageViewModel(_eventBus.Object);
            _target.Init(Command.Help);

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

