namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class AttractCustomizationViewModelTests
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IAttractConfigurationProvider> _attractProvider;
        private Mock<IGameProvider> _gameProvider;

        private List<IAttractInfo> _attractInfo;
        private List<IGameDetail> _gameDetail;

        private AttractCustomizationViewModel _target;
        private dynamic _accessor;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _attractProvider =
                MoqServiceManager.CreateAndAddService<IAttractConfigurationProvider>(MockBehavior.Strict);
            _gameProvider = MoqServiceManager.CreateAndAddService<IGameProvider>(MockBehavior.Strict);

            _gameDetail = MockGameInfo.GetMockGameDetailInfo().ToList();
            _gameProvider.Setup(g => g.GetAllGames()).Returns(_gameDetail);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
            _propertiesManager.Setup(p => p.SetProperty(It.IsAny<string>(), It.IsAny<object>()));

            _attractInfo = MockGameInfo.GetMockAttractInfo().ToList();
            _attractProvider.Setup(a => a.GetAttractSequence()).Returns(_attractInfo);

            _eventBus.Setup(
                e => e.Subscribe(
                    It.IsAny<AttractCustomizationViewModel>(),
                    It.IsAny<Action<OperatorMenuExitingEvent>>()));

            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SlotAttractSelected, It.IsAny<bool>())).Returns(!_attractInfo.Any(g => g.GameType == GameType.Slot && !g.IsSelected));
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.KenoAttractSelected, It.IsAny<bool>())).Returns(!_attractInfo.Any(g => g.GameType == GameType.Keno && !g.IsSelected));
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.PokerAttractSelected, It.IsAny<bool>())).Returns(!_attractInfo.Any(g => g.GameType == GameType.Poker && !g.IsSelected));
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.BlackjackAttractSelected, It.IsAny<bool>())).Returns(!_attractInfo.Any(g => g.GameType == GameType.Blackjack && !g.IsSelected));
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.RouletteAttractSelected, It.IsAny<bool>())).Returns(!_attractInfo.Any(g => g.GameType == GameType.Roulette && !g.IsSelected));
            _propertiesManager
                .Setup(p => p.GetProperty(GamingConstants.DefaultAttractSequenceOverridden, It.IsAny<bool>()))
                .Returns(true);

        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void AttractCustomizationViewModelTest()
        {
            var target = new AttractCustomizationViewModel();
            Assert.IsNotNull(target);
            Assert.AreEqual(_attractInfo.Count, target.ConfiguredAttractInfo.Count);
        }

        [DataRow(GameType.Slot, false)]
        [DataRow(GameType.Keno, false)]
        [DataRow(GameType.Poker, false)]
        [DataRow(GameType.Blackjack, false)]
        [DataRow(GameType.Roulette, false)]
        [DataRow(GameType.Slot, true)]
        [DataRow(GameType.Keno, true)]
        [DataRow(GameType.Poker, true)]
        [DataRow(GameType.Blackjack, true)]
        [DataRow(GameType.Roulette, true)]
        [DataTestMethod]
        public void WhenGameTypeDisabledNoAttractSelectedForThatGameType(GameType gameType, bool selected)
        {
            CreateTarget();
            SelectAttract(gameType, selected);
            if (selected)
            {
                Assert.IsFalse(_target.ConfiguredAttractInfo.Any(g => g.GameType == gameType && !g.IsSelected));
            }
            else
            {
                Assert.IsFalse(_target.ConfiguredAttractInfo.Any(g => g.GameType == gameType && g.IsSelected));
            }

            switch (gameType)
            {
                case GameType.Slot:
                    if (selected)
                    {
                        Assert.IsTrue(_target.SlotAttractSelected);
                    }
                    else
                    {
                        Assert.IsFalse(_target.SlotAttractSelected);
                    }

                    break;
                case GameType.Keno:
                    if (selected)
                    {
                        Assert.IsTrue(_target.KenoAttractSelected);
                    }
                    else
                    {
                        Assert.IsFalse(_target.KenoAttractSelected);
                    }

                    break;
                case GameType.Poker:

                    if (selected)
                    {
                        Assert.IsTrue(_target.PokerAttractSelected);
                    }
                    else
                    {
                        Assert.IsFalse(_target.PokerAttractSelected);
                    }

                    break;
                case GameType.Blackjack:
                    if (selected)
                    {
                        Assert.IsTrue(_target.BlackjackAttractSelected);
                    }
                    else
                    {
                        Assert.IsFalse(_target.BlackjackAttractSelected);
                    }

                    break;
                case GameType.Roulette:
                    if (selected)
                    {
                        Assert.IsTrue(_target.RouletteAttractSelected);
                    }
                    else
                    {
                        Assert.IsFalse(_target.RouletteAttractSelected);
                    }

                    break;
            }
        }

        [DataRow(GameType.Slot, false)]
        [DataRow(GameType.Keno, false)]
        [DataRow(GameType.Poker, false)]
        [DataRow(GameType.Blackjack, false)]
        [DataRow(GameType.Roulette, false)]
        [DataRow(GameType.Slot, true)]
        [DataRow(GameType.Keno, true)]
        [DataRow(GameType.Poker, true)]
        [DataRow(GameType.Blackjack, true)]
        [DataRow(GameType.Roulette, true)]
        [DataTestMethod]
        public void WhenGameTypeDisabledAttractCheckboxOptionDisabled(GameType gameType, bool enabled)
        {
            CreateTarget();
            SelectGameDetail(gameType, enabled);
            switch (gameType)
            {
                case GameType.Slot:
                    if (enabled)
                    {
                        Assert.IsTrue(_target.SlotAttractOptionEnabled);
                    }
                    else
                    {
                        Assert.IsFalse(_target.SlotAttractOptionEnabled);
                    }

                    break;
                case GameType.Keno:
                    if (enabled)
                    {
                        Assert.IsTrue(_target.KenoAttractOptionEnabled);
                    }
                    else
                    {
                        Assert.IsFalse(_target.KenoAttractOptionEnabled);
                    }

                    break;
                case GameType.Poker:

                    if (enabled)
                    {
                        Assert.IsTrue(_target.PokerAttractOptionEnabled);
                    }
                    else
                    {
                        Assert.IsFalse(_target.PokerAttractOptionEnabled);
                    }

                    break;
                case GameType.Blackjack:
                    if (enabled)
                    {
                        Assert.IsTrue(_target.BlackjackAttractOptionEnabled);
                    }
                    else
                    {
                        Assert.IsFalse(_target.BlackjackAttractOptionEnabled);
                    }

                    break;
                case GameType.Roulette:
                    if (enabled)
                    {
                        Assert.IsTrue(_target.RouletteAttractOptionEnabled);
                    }
                    else
                    {
                        Assert.IsFalse(_target.RouletteAttractOptionEnabled);
                    }

                    break;
            }
        }

        [DataRow(AttractCustomizationViewModel.MoveBehavior.MoveUp)]
        [DataRow(AttractCustomizationViewModel.MoveBehavior.MoveDown)]
        [DataRow(AttractCustomizationViewModel.MoveBehavior.MoveToTop)]
        [DataRow(AttractCustomizationViewModel.MoveBehavior.MoveToBottom)]
        [DataTestMethod]
        public void WhenAttractOrderIsChangedEnsureThereAreChangesToSave(
            AttractCustomizationViewModel.MoveBehavior behavior)
        {
            var totalItems = _attractInfo.Count();
            CreateTarget();
            for (var i = 0; i < totalItems; ++i)
            {
                var newIndex = -1;
                switch (behavior)
                {
                    case AttractCustomizationViewModel.MoveBehavior.MoveUp:
                        _target.SelectedItem = _target.ConfiguredAttractInfo.ElementAt(i);
                        _target.MoveUpCommand?.Execute(null);
                        newIndex = i == 0 ? 0 : i - 1;
                        if (i != 0)
                        {
                            Assert.IsTrue(_accessor.HasChanges());
                        }

                        break;
                    case AttractCustomizationViewModel.MoveBehavior.MoveDown:
                        _target.SelectedItem = _target.ConfiguredAttractInfo.ElementAt(i);
                        _target.MoveDownCommand?.Execute(null);
                        newIndex = i == totalItems - 1 ? totalItems - 1 : i + 1;
                        if (i != totalItems - 1)
                        {
                            Assert.IsTrue(_accessor.HasChanges());
                        }

                        break;
                    case AttractCustomizationViewModel.MoveBehavior.MoveToTop:
                        _target.SelectedItem = _target.ConfiguredAttractInfo.ElementAt(i);
                        _target.MoveToTopCommand?.Execute(null);
                        newIndex = 0;
                        if (i != 0)
                        {
                            Assert.IsTrue(_accessor.HasChanges());
                        }

                        break;
                    case AttractCustomizationViewModel.MoveBehavior.MoveToBottom:
                        _target.SelectedItem = _target.ConfiguredAttractInfo.ElementAt(i);
                        _target.MoveToBottomCommand?.Execute(null);
                        newIndex = totalItems - 1;
                        if (i != totalItems - 1)
                        {
                            Assert.IsTrue(_accessor.HasChanges());
                        }

                        break;
                }

                Assert.AreEqual(_target.SelectedItem, _target.ConfiguredAttractInfo.ElementAt(newIndex));
            }
        }

        [TestMethod]
        public void WhenAttractItemSelectionIsChangedEnsureThereAreChangesToSave()
        {
            CreateTarget();
            
            Assert.IsFalse(_accessor.HasChanges());
            SelectAttract(_target.ConfiguredAttractInfo.First().ThemeId);
            Assert.IsTrue(_accessor.HasChanges());
        }

        [TestMethod]
        public void WhenAttractItemSelectionIsChangedEnsureThereAreNoChangesToSaveAfterSaving()
        {
            CreateTarget();
            _attractProvider.Setup(a => a.SaveAttractSequence(It.IsAny<ICollection<IAttractInfo>>()));

            Assert.IsFalse(_accessor.HasChanges());
            SelectAttract(_target.ConfiguredAttractInfo.First().ThemeId);
            Assert.IsTrue(_accessor.HasChanges());
            _accessor.Save();

            // Simulate property saved.
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SlotAttractSelected, It.IsAny<bool>()))
                .Returns(_target.SlotAttractSelected);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.KenoAttractSelected, It.IsAny<bool>()))
                .Returns(_target.KenoAttractSelected);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.PokerAttractSelected, It.IsAny<bool>()))
                .Returns(_target.PokerAttractSelected);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.BlackjackAttractSelected, It.IsAny<bool>()))
                .Returns(_target.BlackjackAttractOptionEnabled);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.RouletteAttractSelected, It.IsAny<bool>()))
                .Returns(_target.RouletteAttractOptionEnabled);

            Assert.IsFalse(_accessor.HasChanges());
        }

        [TestMethod]
        public void WhenAttractItemSelectionNotChangedEnsureThereAreNoChangesToSave()
        {
            CreateTarget();
            Assert.IsFalse(_accessor.HasChanges());
            SelectAttract(_target.ConfiguredAttractInfo.First().ThemeId);
            SelectAttract(_target.ConfiguredAttractInfo.First().ThemeId, true);
            Assert.IsFalse(_accessor.HasChanges());
        }

        [TestMethod]
        public void WhenGameTypeIsUncheckedAllGamesAreUncheckedFromAttractSequence()
        {
            CreateTarget();
            Assert.IsFalse(_accessor.HasChanges());

            SelectAttract(GameType.Slot, true);
            Assert.IsTrue(_accessor.SlotAttractSelected);

            _accessor.SlotAttractSelected = false;

            Assert.IsFalse(_target.ConfiguredAttractInfo.Any(g => g.GameType == GameType.Slot && g.IsSelected));
            Assert.IsTrue(_accessor.HasChanges());
        }

        [DataTestMethod]
        [DataRow(GameType.Slot)]
        [DataRow(GameType.Keno)]
        [DataRow(GameType.Poker)]
        [DataRow(GameType.Blackjack)]
        [DataRow(GameType.Roulette)]
        public void WhenAnyAttractGameIsUncheckedTheGameTypeIsUnchecked(GameType type)
        {
            CreateTarget();

            Assert.IsFalse(_accessor.HasChanges());
            SelectAttract(type, true);

            switch (type)
            {
                case GameType.Slot:
                    Assert.IsTrue(_accessor.SlotAttractSelected);

                    break;
                case GameType.Keno:
                    Assert.IsTrue(_accessor.KenoAttractSelected);

                    break;
                case GameType.Poker:
                    Assert.IsTrue(_accessor.PokerAttractSelected);

                    break;
                case GameType.Blackjack:
                    Assert.IsTrue(_accessor.BlackjackAttractSelected);

                    break;
                case GameType.Roulette:
                    Assert.IsTrue(_accessor.RouletteAttractSelected);

                    break;
            }
            var game = _target.ConfiguredAttractInfo.FirstOrDefault(g => g.GameType == type);
            game.IsSelected = false;

            Assert.IsTrue(_accessor.HasChanges());

            switch (type)
            {
                case GameType.Slot:
                    Assert.IsFalse(_accessor.SlotAttractSelected);

                    break;
                case GameType.Keno:
                    Assert.IsFalse(_accessor.KenoAttractSelected);

                    break;
                case GameType.Poker:
                    Assert.IsFalse(_accessor.PokerAttractSelected);

                    break;
                case GameType.Blackjack:
                    Assert.IsFalse(_accessor.BlackjackAttractSelected);

                    break;
                case GameType.Roulette:
                    Assert.IsFalse(_accessor.RouletteAttractSelected);

                    break;
            }
        }

        private void SelectAttract(string themeId, bool selected = false)
        {
            var attractInfo = _target.ConfiguredAttractInfo.Where(
                ai => ai.ThemeId == themeId);
            Assert.IsNotNull(attractInfo);
            var attractItem = attractInfo.First();
            attractItem.IsSelected = selected;
        }

        private void SelectAttract(GameType gameType, bool selected = false)
        {
            var attractInfo = _target.ConfiguredAttractInfo.Where(
                ai => ai.GameType == gameType).ToList();
            foreach (var ai in attractInfo)
            {
                ai.IsSelected = selected;
            }
        }

        private void SelectGameDetail(GameType gameType, bool enabled = false)
        {
            var attractInfo = _gameDetail.Where(
                gd => gd.GameType == gameType).ToList();
            foreach (var ai in attractInfo)
            {
                ((MockGameInfo)ai).Active = enabled;
            }
        }

        private void CreateTarget()
        {
            _target = new AttractCustomizationViewModel();
            _accessor = new DynamicPrivateObject(_target);
        }
    }
}
