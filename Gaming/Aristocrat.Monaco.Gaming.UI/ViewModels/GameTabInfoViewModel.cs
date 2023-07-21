namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Contracts;
    using Contracts.Models;
    using Models;
    using Monaco.UI.Common.Extensions;

    /// <summary>
    ///     The View Model used for binding lobby game tabs
    /// </summary>
    public class GameTabInfoViewModel : BaseObservableObject
    {
        private const double SubTabsTopMargin = 158;
        private readonly double[] _subTabsLeftMargin = { 350, 650, 950, 1250, 855, 1145 };

        private List<GameTabInfo> _tabs = new List<GameTabInfo>();

        public List<GameTabInfo> Tabs
        {
            get => _tabs;
            set
            {
                if (_tabs == value)
                {
                    return;
                }

                _tabs = value;
                UpdateTabInfo();
            }
        }

        public ObservableCollection<DenominationInfoViewModel> Denominations { get; } =
            new ObservableCollection<DenominationInfoViewModel>();

        public ObservableCollection<SubTabInfoViewModel> SubTabs { get; } =
            new ObservableCollection<SubTabInfoViewModel>();

        public long SelectedDenomination => Denominations.FirstOrDefault(o => o.IsSelected)?.Denomination ?? 0;

        public string SelectedSubTabText => SelectedSubTab?.TypeText ?? string.Empty;

        public int TabCount => _tabs.Count;

        public IEnumerable<GameCategory> Categories => _tabs.Select(o => o.Category);

        public Action<GameCategory> GameTypeChanged { get; set; }

        public Action<long> DenominationChanged { get; set; }

        public Action<string> SubTabChanged { get; set; }

        public GameCategory SelectedCategory =>
            _tabs.Count > 0 ? _tabs[SelectedTabIndex].Category : GameCategory.Undefined;

        public Thickness SubTabsMargin
        {
            get
            {
                var left = SelectedTabIndex < _subTabsLeftMargin.Length ? _subTabsLeftMargin[SelectedTabIndex] : 0;
                return new Thickness(left, SubTabsTopMargin, 0, 0);
            }
        }

        public SubTabInfoViewModel SelectedSubTab => SubTabs.FirstOrDefault(o => o.IsSelected);

        private int SelectedTabIndex => _tabs.FirstOrDefault(o => o.Enabled)?.TabIndex ?? 0;

        /// <summary>
        ///     Generates the tabs from the enabled game categories
        /// </summary>
        /// <param name="gameInfo">The list of available GameInfos</param>
        public void SetupGameTypeTabs(IEnumerable<GameInfo> gameInfo)
        {
            var lastSelectedCategory = SelectedCategory;
            var gameList = gameInfo.Where(o => o.Enabled).ToList();
            var gameTabs = new List<GameTabInfo>();
            var tabIndex = 0;
            var categoryTabOrder = new List<GameCategory>
            {
                GameCategory.LightningLink,
                GameCategory.Slot,
                GameCategory.Poker,
                GameCategory.MultiDrawPoker,
                GameCategory.Keno,
                GameCategory.Table
            };

            var tabs = categoryTabOrder.Intersect(
                gameList.Select(
                    x =>
                    {
                        if (x.Category != GameCategory.Undefined)
                        {
                            return x.Category;
                        }

                        return ConvertGameToDefaultCategory(x.GameType);
                    }));

            foreach (var category in tabs)
            {
                gameTabs.Add(new GameTabInfo(category, tabIndex));
                tabIndex++;
            }

            if (gameTabs.Count > 0)
            {
                var selectedTabIndex = lastSelectedCategory != GameCategory.Undefined
                    ? gameTabs.FirstOrDefault(o => o.Category == lastSelectedCategory)?.TabIndex ?? 0
                    : 0;

                gameTabs[selectedTabIndex].Enabled = true;
            }

            Tabs = gameTabs;
        }

        /// <summary>
        ///     Selects a tab
        /// </summary>
        /// <param name="selectedTab">The GameTabInfo to select</param>
        public void SelectTab(GameTabInfo selectedTab)
        {
            Tabs.ForEach(o => o.Enabled = o == selectedTab);
            UpdateTabInfo();
            GameTypeChanged?.Invoke(SelectedCategory);
        }

        /// <summary>
        ///     Selects a tab
        /// </summary>
        /// <param name="index">The index of the GameTabInfo to select</param>
        public void SelectTab(int index)
        {
            if (index >= 0 && index < TabCount && index != SelectedTabIndex)
            {
                SelectTab(Tabs[index]);
            }
        }

        /// <summary>
        ///     Sets the available shared denomination buttons for this tab
        /// </summary>
        /// <param name="denominations">A list of denominations</param>
        public void SetDenominations(IEnumerable<long> denominations)
        {
            Denominations.Clear();
            Denominations.AddRange(denominations.OrderBy(x => x).Select(x => new DenominationInfoViewModel(x)));
            SetSelectedDenomination(Denominations.FirstOrDefault(), false);
        }

        /// <summary>
        ///     Sets the selected denomination button
        /// </summary>
        /// <param name="denomination">The DenominationInfoViewModel to select</param>
        /// <param name="notify">Defaults to true to raise a DenominationChanged event</param>
        public void SetSelectedDenomination(DenominationInfoViewModel denomination, bool notify = true)
        {
            foreach (var denom in Denominations)
            {
                denom.IsSelected = false;
            }

            if (denomination != null)
            {
                denomination.IsSelected = true;
                if (notify)
                {
                    DenominationChanged?.Invoke(denomination.Denomination);
                }
            }
        }

        /// <summary>
        ///     Sets the available sub-tabs for the currently selected tab
        /// </summary>
        /// <param name="types">A list of sub-tab type strings</param>
        public void SetSubTabs(IEnumerable<string> types)
        {
            SubTabs.Clear();
            foreach (var subTab in types.Where(x => !string.IsNullOrEmpty(x)).Select(x => new SubTabInfoViewModel(x))
                .OrderBy(o => o.Type))
            {
                SubTabs.Add(subTab);
            }

            if (SubTabs.Count > 0)
            {
                SelectSubTab(SubTabs[0], false);
                var visible = SubTabs.Count > 1;

                foreach (var subTab in SubTabs)
                {
                    subTab.IsVisible = visible;
                }
            }
        }

        /// <summary>
        ///     Selects a sub-tab
        /// </summary>
        /// <param name="selectedSubTab">The SubTabInfoViewModel to select</param>
        /// <param name="notify">Defaults to true to raise a SubTabChanged event</param>
        public void SelectSubTab(SubTabInfoViewModel selectedSubTab, bool notify = true)
        {
            if (selectedSubTab == null && SubTabs.Any())
            {
                selectedSubTab = SubTabs.First();
            }

            foreach (var subTab in SubTabs)
            {
                subTab.IsSelected = false;
            }

            if (selectedSubTab != null)
            {
                selectedSubTab.IsSelected = true;
                if (notify)
                {
                    SubTabChanged?.Invoke(selectedSubTab.TypeText);
                }
            }
        }

        /// <summary>
        ///     Sets a default selected tab
        /// </summary>
        public void SetDefaultTab()
        {
            var defaultPriorityOrder = new List<GameCategory>
            {
                GameCategory.LightningLink,
                GameCategory.Slot,
                GameCategory.Keno,
                GameCategory.Poker,
                GameCategory.Table,
                GameCategory.MultiDrawPoker
            };
            var index = _tabs.FindIndex(
                y => y.Category == defaultPriorityOrder.Intersect(_tabs.Select(x => x.Category)).FirstOrDefault());

            SelectTab(index == -1 ? 0 : index);
            SelectSubTab(null);
        }

        /// <summary>
        ///     Selects the next denomination available on this tab
        /// </summary>
        public void IncrementSelectedDenomination()
        {
            var selectedDenomination = Denominations.FirstOrDefault(o => o.IsSelected);
            if (selectedDenomination != null)
            {
                var currentIndex = Denominations.IndexOf(selectedDenomination);
                currentIndex = (currentIndex + 1) % Denominations.Count;
                SetSelectedDenomination(Denominations[currentIndex]);
            }
        }

        /// <summary>
        ///     Switches to the Next or Previous available tab
        ///     If the tab has sub-tabs, those will be traversed as well
        /// </summary>
        /// <param name="next">True to go to next; False to go to previous</param>
        public void NextPreviousTab(bool next)
        {
            if (SubTabs.Any())
            {
                // First attempt to navigate through sub tabs
                // If we have reached the end, continue to next main Tab
                var selectedSubTabIndex = SubTabs.IndexOf(SelectedSubTab);

                if (next && selectedSubTabIndex < SubTabs.Count - 1)
                {
                    // Go to next sub tab
                    SelectSubTab(selectedSubTabIndex + 1);
                    return;
                }

                if (!next && selectedSubTabIndex > 0)
                {
                    // Go to previous sub tab
                    SelectSubTab(selectedSubTabIndex - 1);
                    return;
                }
            }

            var selectedTabIndex = SelectedTabIndex;

            selectedTabIndex = next ? (selectedTabIndex + 1) % TabCount : selectedTabIndex - 1;

            if (selectedTabIndex < 0)
            {
                selectedTabIndex = TabCount - 1;
            }

            SelectTab(selectedTabIndex);
        }

        public GameCategory ConvertGameToDefaultCategory(GameType id)
        {
            switch (id)
            {
                case GameType.Slot:
                    return GameCategory.Slot;
                case GameType.Blackjack:
                    return GameCategory.Table;
                case GameType.Keno:
                    return GameCategory.Keno;
                case GameType.Poker:
                    return GameCategory.Poker;
                case GameType.Roulette:
                    return GameCategory.Table;
                case GameType.LightningLink:
                    return GameCategory.LightningLink;
                default:
                    return GameCategory.Undefined;
            }
        }

        public IList<GameType> ConvertCategoryToDefaultGame(GameCategory id)
        {
            switch (id)
            {
                case GameCategory.Slot:
                    return new List<GameType> { GameType.Slot };
                case GameCategory.Keno:
                    return new List<GameType> { GameType.Keno };
                case GameCategory.Poker:
                    return new List<GameType> { GameType.Poker };
                case GameCategory.Table:
                    return new List<GameType> { GameType.Blackjack, GameType.Roulette };
                case GameCategory.LightningLink:
                    return new List<GameType> { GameType.LightningLink };
                default:
                    return new List<GameType> { GameType.Undefined };
            }
        }

        private void SelectSubTab(int index)
        {
            var selectedIndex = SubTabs.IndexOf(SelectedSubTab);
            if (index >= 0 && index < SubTabs.Count && index != selectedIndex)
            {
                SelectSubTab(SubTabs[index]);
            }
        }

        private void UpdateTabInfo()
        {
            OnPropertyChanged(nameof(Tabs));
            OnPropertyChanged(nameof(TabCount));
            OnPropertyChanged(nameof(SelectedTabIndex));
            OnPropertyChanged(nameof(SubTabs));
            OnPropertyChanged(nameof(SubTabsMargin));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(GameTypes));
        }
    }

    public class GameTabInfo : BaseObservableObject
    {
        private const string TabResourceKeyPrefix = "GameTab";
        private const string LabelResourceKeyPrefix = "Tab";
        private const string Disabled = "Disabled";
        private const string ImagesPath = "..\\jurisdiction\\DefaultAssets\\ui\\Images\\";
        private bool _enabled;

        public GameTabInfo(GameCategory category, int tabIndex)
        {
            Category = category;
            TabIndex = tabIndex;
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }

                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
                OnPropertyChanged(nameof(LabelResourceKey));
                OnPropertyChanged(nameof(LabelAnimatedResourcePath));
                OnPropertyChanged(nameof(HasAnimatedResource));
                OnPropertyChanged(nameof(TabResourceKey));
            }
        }

        public int TabIndex { get; }

        public GameCategory Category { get; }

        public string LabelResourceKey => $"{LabelResourceKeyPrefix}{Category}{DisabledText}";

        public string LabelAnimatedResourcePath => $"{AppDomain.CurrentDomain.BaseDirectory}{ImagesPath}{LabelResourceKey}.gif";

        public bool HasAnimatedResource => File.Exists(LabelAnimatedResourcePath);

        public string TabResourceKey => $"{TabResourceKeyPrefix}{TabIndex + 1}{DisabledText}";

        private string DisabledText => Enabled ? string.Empty : Disabled;
    }
}
