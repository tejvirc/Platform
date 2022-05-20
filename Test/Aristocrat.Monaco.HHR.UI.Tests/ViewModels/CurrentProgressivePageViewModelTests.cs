namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Kernel;
    using Menu;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.Button;
    using Hhr.UI.ViewModels;
    using Test.Common;
    using Moq;

    [TestClass]
    public class CurrentProgressivePageViewModelTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;

        private CurrentProgressivePageViewModel _target;

        private int _buttonId = -1;

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _protocolLinkedProgressiveAdapter =
                MoqServiceManager.CreateAndAddService<IProtocolLinkedProgressiveAdapter>(MockBehavior.Strict);

            _eventBus.Setup(m => m.Publish(It.IsAny<DownEvent>())).Callback((DownEvent ev) => { _buttonId = ev.LogicalId; });
            _protocolLinkedProgressiveAdapter.Setup(l => l.ViewLinkedProgressiveLevels())
                .Returns(new List<LinkedProgressiveLevel>());

            _target = CreateCurrentProgressivePageViewModel();

            // Reset the bet up/down delay counter so that we don't hit the limit while testing.
            HostPageViewModelManager.ResetBetButtonDelayLimit(
                DateTime.UtcNow.Ticks - HostPageViewModelManager.BetUpDownDelayTimeTicks - 1);

            if (Application.Current == null)
            {
                _ = new Application();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, DisplayName = "Null EventBus")]
        [DataRow(false, true, DisplayName = "Null LinkedProgressiveProvider")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_InvalidParams_ThrowException(bool nullEventBus, bool nullLinkedProgressiveProvider)
        {
            _ = CreateCurrentProgressivePageViewModel(nullEventBus, nullLinkedProgressiveProvider);
        }

        [TestMethod]
        public void LoadingProgressives_InitNotInvoked_PoolsNotLoaded()
        {
            Assert.AreEqual(0, _target.ProgressivePools.Count);
        }

        [TestMethod]
        public void LoadingProgressives_LinkedProgressiveLevelsAvailable_ProgressivePoolsLoaded()
        {
            var levels = CreateProgressiveLevels();

            _protocolLinkedProgressiveAdapter.Setup(l => l.GetActiveProgressiveLevels()).Returns(levels);

            _target.Init(Command.CurrentProgressive);

            Assert.AreEqual(levels.GroupBy(l => l.WagerCredits).Count(), _target.ProgressivePools.Count);
        }

        [TestMethod]
        public void ClickButton_BetUp_ExpectDownEventWithBetUP()
        {
            _target.Init(Command.Bet);
            _target.Commands.First(c => c.Command == Command.Bet).Execute(Command.BetUp);
            Assert.AreEqual((int)ButtonLogicalId.BetUp, _buttonId);
        }

        [TestMethod]
        public void ClickButton_BetDown_ExpectDownEventWithBetDown()
        {
            _target.Init(Command.Bet);
            _target.Commands.First(c => c.Command == Command.Bet).Execute(Command.BetDown);
            Assert.AreEqual((int)ButtonLogicalId.BetDown, _buttonId);
        }

        private List<ProgressiveLevel> CreateProgressiveLevels()
        {
            return new List<ProgressiveLevel>
            {
                CreateProgressiveLevel(40, 1000),
                CreateProgressiveLevel(40, 500),
                CreateProgressiveLevel(80, 900),
                CreateProgressiveLevel(80, 800)
            };
        }


        private static ProgressiveLevel CreateProgressiveLevel(long wager, long currentAmount)
        {
            return new ProgressiveLevel
            {
                WagerCredits = wager,
                CurrentValue = currentAmount
            };
        }

        private CurrentProgressivePageViewModel CreateCurrentProgressivePageViewModel(bool nullEventBus = false,
            bool nullLinkedProgressiveProvider = false)
        {
            return new CurrentProgressivePageViewModel(nullEventBus ? null : _eventBus.Object,
                nullLinkedProgressiveProvider ? null : _protocolLinkedProgressiveAdapter.Object);
        }
    }
}