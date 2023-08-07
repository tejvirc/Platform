namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using log4net;
    using ManagedBink;
    using Monaco.UI.Common.Extensions;
    using MVVM.Model;
    using ViewModels;
    using BitmapImage = System.Windows.Media.Imaging.BitmapImage;
    using Size = System.Windows.Size;

    /// <summary>
    ///     Defines the GameInfo class
    /// </summary>
    [CLSCompliant(false)]
    public class GameInfo : BaseNotify, IGameInfo, IAttractDetails
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        // Default resource key for denomination button panel
        private const string DenomPanelDefaultKey = "DenominationPanel";

        private string _attractHighlightVideoPath = string.Empty;
        private string _bottomAttractVideoPath = string.Empty;
        private long _denom = 1;
        private DenominationInfoViewModel _selectedDenomination;
        private string _betOption = string.Empty;
        private long _filteredDenom = 1;
        private string _dllPath;
        private bool _enabled;
        private GameType _gameType = GameType.Slot;
        private string _gameSubtype;
        private GameCategory _category;
        private GameSubCategory _subCategory;
        private bool _isSelected;
        private string _imagePath;
        private string _denomButtonPath;
        private string _denomButtonResourceKey;
        private string _denomPanelPath;

        // Used to determine if "new" game.  For example, a requirement is that
        // we want to overlay a "new" icon over a game if it has been added in
        // the past 90 days.
        private DateTime _installDateTime = DateTime.UtcNow;

        private bool _isNew;
        private string _loadingScreenPath = string.Empty;

        private string _name;
        private Size _gameIconSize = Size.Empty;
        private bool _platinumSeries;

        private string _progressiveOrBonusValue;
        private string _topAttractVideoPath = string.Empty;
        private string _topperAttractVideoPath;
        private bool _progressiveErrorVisible;
        private string _progressiveIndicatorText;
        private ProgressiveLobbyIndicator _progressiveIndicator;

        /// <summary>
        ///     Gets or sets the game id
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        ///     Gets or sets the path to the game dll
        /// </summary>
        public string DllPath
        {
            get => _dllPath;
            set => SetProperty(ref _dllPath, value);
        }

        /// <summary>
        ///     Gets or sets the game name
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        ///     Gets the default game icon size from the image or bink video
        /// </summary>
        public Size GameIconSize
        {
            get
            {
                if (_gameIconSize != Size.Empty || string.IsNullOrEmpty(_imagePath))
                {
                    return _gameIconSize;
                }

                if (ImageIsBink)
                {
                    var bink = new BinkVideoDecoder();
                    bink.Open(_imagePath);
                    _gameIconSize = new Size(bink.VideoWidth, bink.VideoHeight);
                    bink.Dispose();
                }
                else
                {
                    var image = Image.FromFile(_imagePath);
                    _gameIconSize = new Size(image.Width, image.Height);
                    image.Dispose();
                }

                return _gameIconSize;
            }
        }

        /// <summary>
        ///     Gets whether the image for this game is a bink video
        /// </summary>
        public bool ImageIsBink => ImagePath?.EndsWith(".bk2") ?? false;

        /// <summary>
        ///     Gets or sets the game image path
        /// </summary>
        public string ImagePath
        {
            get
            {
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

                if (!string.IsNullOrEmpty(TopPickImagePath) && propertiesManager.GetValue(GamingConstants.ShowTopPickBanners, true))
                {
                    return TopPickImagePath;
                }
                return _imagePath;
            }

            set => SetProperty(ref _imagePath, value, nameof(ImagePath), nameof(ImageIsBink));
        }

        /// <summary>
        ///     Gets or sets the game image path
        /// </summary>
        public string TopPickImagePath;

        /// <summary>
        ///     Gets or sets the top screen attract video path
        /// </summary>
        public string TopAttractVideoPath
        {
            get => _topAttractVideoPath;
            set => SetProperty(ref _topAttractVideoPath, value);
        }

        /// <summary>
        ///     Gets or sets the topper screen attract video path
        /// </summary>
        public string TopperAttractVideoPath
        {
            get => _topperAttractVideoPath;
            set => SetProperty(ref _topperAttractVideoPath, value);
        }

        /// <summary>
        ///     Gets or sets the bottom screen attract video path
        /// </summary>
        public string BottomAttractVideoPath
        {
            get => _bottomAttractVideoPath;
            set => SetProperty(ref _bottomAttractVideoPath, value);
        }

        /// <summary>
        ///     Gets or sets the image to show when a game is loading
        /// </summary>
        public string LoadingScreenPath
        {
            get => _loadingScreenPath;
            set => SetProperty(ref _loadingScreenPath, value);
        }

        /// <summary>
        ///     Gets or sets the attract highlight video path.  This can be data driven
        ///     and vary per game; however, right now the lobby sets it based on a global setting.
        /// </summary>
        public string AttractHighlightVideoPath
        {
            get => _attractHighlightVideoPath;
            set => SetProperty(ref _attractHighlightVideoPath, value);
        }

        /// <summary>
        ///     Gets or sets the game progressive value
        /// </summary>
        public string ProgressiveOrBonusValue
        {
            get => _progressiveOrBonusValue;
            set => SetProperty(ref _progressiveOrBonusValue, value);
        }

        public bool HasProgressiveLabelDisplay => ProgressiveIndicator != ProgressiveLobbyIndicator.Disabled;

        public ProgressiveLobbyIndicator ProgressiveIndicator
        {
            get => _progressiveIndicator;
            set => SetProperty(
                ref _progressiveIndicator,
                value,
                nameof(ProgressiveIndicator),
                nameof(HasProgressiveLabelDisplay),
                nameof(IsSelectedWithProgressiveLabel),
                nameof(ProgressiveIndicatorText));
        }

        public bool ProgressiveErrorVisible
        {
            get => _progressiveErrorVisible;
            set => SetProperty(ref _progressiveErrorVisible, value);
        }

        public string ProgressiveIndicatorText
        {
            get => _progressiveIndicatorText;
            set => SetProperty(ref _progressiveIndicatorText, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the game has a progressive bonus
        /// </summary>
        public bool HasProgressiveOrBonusValue => !string.IsNullOrWhiteSpace(ProgressiveOrBonusValue);

        public string DenomButtonPath
        {
            get => _denomButtonPath;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || _denomButtonPath == value)
                {
                    return;
                }

                var key = $"{ThemeId}_Button";
                _denomButtonResourceKey = key;
                _denomButtonPath = value;

                try
                {
                    // Add the bitmap to the resource dictionary so it can be accessed by key with ImageHelper
                    if (!Application.Current.Resources.Contains(key))
                    {
                        var uri = new Uri(_denomButtonPath, UriKind.Absolute);
                        Application.Current.Resources.Add(key, new BitmapImage(uri));
                        Logger.Debug($"{_denomButtonResourceKey} added to resources for custom denom button at path {_denomButtonPath}");
                    }

                    foreach (var denom in Denominations)
                    {
                        denom.DenomButtonSingleOffOverride = _denomButtonResourceKey;
                    }
                }
                catch (Exception ex)
                {
                    _denomButtonResourceKey = null;
                    Logger.Warn($"Failed to set bitmap image at path {_denomButtonPath}", ex);
                }
            }
        }

        public bool HasCustomDenomPanel => !string.IsNullOrWhiteSpace(DenomPanelKey);

        public string DenomPanelPath
        {
            get => _denomPanelPath;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || _denomPanelPath == value)
                {
                    return;
                }

                var key = $"{ThemeId}_Panel";
                DenomPanelKey = key;
                _denomPanelPath = value;

                try
                {
                    // Add the bitmap to the resource dictionary so it can be accessed by key with ImageHelper
                    if (!Application.Current.Resources.Contains(key))
                    {
                        var uri = new Uri(_denomPanelPath, UriKind.Absolute);
                        Application.Current.Resources.Add(key, new BitmapImage(uri));
                        Logger.Debug($"{DenomPanelKey} added to resources for custom denom panel at path {_denomPanelPath}");
                    }
                }
                catch (Exception ex)
                {
                    DenomPanelKey = null;
                    Logger.Warn($"Failed to set bitmap image at path {_denomPanelPath}", ex);
                }
            }
        }

        public string DenomPanelKey { get; private set; }

        /// <summary>
        ///     Gets or sets the denomination
        /// </summary>
        public long Denomination
        {
            get => _denom;
            set => SetProperty(ref _denom, value);
        }

        /// <summary>
        ///     Gets or sets the selected denomination
        /// </summary>
        public DenominationInfoViewModel SelectedDenomination
        {
            get => _selectedDenomination;
            private set
            {
                SetProperty(ref _selectedDenomination, value);
                foreach (var denom in Denominations)
                {
                    denom.IsSelected = false;
                }

                if (value != null)
                {
                    value.IsSelected = true;
                }
            }
        }

        /// <summary>
        ///     The list of denomination buttons that will appear below the extra large game icons
        /// </summary>
        public ObservableCollection<DenominationInfoViewModel> Denominations { get; } =
            new ObservableCollection<DenominationInfoViewModel>();

        /// <summary>
        ///     Set denominations to display for the premium game
        /// </summary>
        /// <param name="denominations">A list of denomination values to set</param>
        public void SetDenominations(IEnumerable<long> denominations)
        {
            Denominations.Clear();
            Denominations.AddRange(
                denominations.OrderBy(x => x).Select(
                    x => new DenominationInfoViewModel(x)
                    {
                        DenomButtonSingleOffOverride = _denomButtonResourceKey
                    }));

            // Start with all denoms unselected so it doesn't look weird on machines without the VBD denom switching
            SelectedDenomination = null;
        }

        /// <summary>
        ///     Gets the bet option
        /// </summary>
        public string BetOption
        {
            get => _betOption;
            set => SetProperty(ref _betOption, value);
        }

        /// <summary>
        /// Gets or sets the denomination for filtering
        /// </summary>
        public long FilteredDenomination
        {
            get => _filteredDenom;
            set => SetProperty(ref _filteredDenom, value);
        }

        /// <summary>
        ///     Gets or sets the game type
        /// </summary>
        public GameType GameType
        {
            get => _gameType;
            set => SetProperty(ref _gameType, value);
        }

        /// <summary>
        ///     Gets or sets the game type
        /// </summary>
        public string GameSubtype
        {
            get => _gameSubtype;
            set => SetProperty(ref _gameSubtype, value);
        }

        /// <summary>
        ///     Gets or sets whether the game is currently selected in the lobby (visualized with a green border)
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;

            set
            {
                if (SetProperty(ref _isSelected, value, nameof(IsSelected), nameof(IsSelectedWithProgressiveLabel)))
                {
                    SelectedDenomination = null;
                }
            }
        }

        public bool IsSelectedWithProgressiveLabel => IsSelected && HasProgressiveLabelDisplay;

        /// <summary>
        ///     Gets or sets a value indicating whether this is a Platinum Series game
        /// </summary>
        public bool PlatinumSeries
        {
            get => _platinumSeries;
            set => SetProperty(ref _platinumSeries, value);
        }

        /// <summary>
        ///     Gets or sets the date created
        /// </summary>
        public DateTime InstallDateTime
        {
            get => _installDateTime;
            set => SetProperty(ref _installDateTime, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the game is new or not
        /// </summary>
        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the game is enabled or not
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the GameInfo use small or large icons
        /// </summary>
        public bool UseSmallIcons { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating the Locale Graphics associated With the Game Info
        /// </summary>
        public IEnumerable LocaleGraphics { get; set; }

        /// <summary>
        ///     Theme ID of the game.  Not currently displayed anywhere so doesn't need RaisePropertyChanged
        /// </summary>
        public string ThemeId { get; set; }

        /// <summary>
        ///     Holds the game category
        /// </summary>
        public GameCategory Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        /// <summary>
        ///     Holds the game subcategory
        /// </summary>
        public GameSubCategory SubCategory
        {
            get => _subCategory;
            set => SetProperty(ref _subCategory, value);
        }

        /// <summary>
        ///     Select the next denomination available for this game
        /// </summary>
        public void IncrementSelectedDenomination()
        {
            var selectedDenomination = Denominations.FirstOrDefault(o => o.IsSelected);
            if (selectedDenomination != null)
            {
                var currentIndex = Denominations.IndexOf(selectedDenomination);
                currentIndex = (currentIndex + 1) % Denominations.Count;
                SelectedDenomination = Denominations[currentIndex];
            }
            else
            {
                SelectedDenomination = Denominations.FirstOrDefault();
            }
        }

        /// <summary>
        ///     Gets or sets a property stating whether or not this game requires mechanical reels
        /// </summary>
        public bool RequiresMechanicalReels { get; set; }

        /// <summary>
        ///     Select the appropriate image for the Locale Graphics
        /// </summary>
        /// <param name="activeLocaleCode">locale code to use</param>
        public void SelectLocaleGraphics(string activeLocaleCode)
        {
            var localeGraphics = (Dictionary<string, ILocaleGameGraphics>)LocaleGraphics;
            if (localeGraphics == null)
            {
                return;
            }

            if (localeGraphics.ContainsKey(activeLocaleCode))
            {
                if (UseSmallIcons)
                {
                    ImagePath = localeGraphics[activeLocaleCode].SmallIcon;
                    TopPickImagePath = localeGraphics[activeLocaleCode].SmallTopPickIcon;
                }
                else
                {
                    ImagePath = localeGraphics[activeLocaleCode].LargeIcon;
                    TopPickImagePath = localeGraphics[activeLocaleCode].LargeTopPickIcon;
                }

                TopAttractVideoPath = localeGraphics[activeLocaleCode].TopAttractVideo;
                TopperAttractVideoPath = localeGraphics[activeLocaleCode].TopperAttractVideo;
                BottomAttractVideoPath = localeGraphics[activeLocaleCode].BottomAttractVideo;
                LoadingScreenPath = localeGraphics[activeLocaleCode].LoadingScreen;
                DenomButtonPath = localeGraphics[activeLocaleCode].DenomButtonIcon;
                DenomPanelPath = localeGraphics[activeLocaleCode].DenomPanel;
            }
            else
            {
                ImagePath = null;
                TopPickImagePath = null;
                TopAttractVideoPath = null;
                TopperAttractVideoPath = null;
                BottomAttractVideoPath = null;
                LoadingScreenPath = null;
            }
        }

        /// <summary>
        /// Gets the top attract video for the game for the specified locale code
        /// </summary>
        /// <param name="localeCode"></param>
        /// <returns></returns>
        public string GetTopAttractVideoPathByLocaleCode(string localeCode)
        {
            var localeGraphics = (Dictionary<string, ILocaleGameGraphics>)LocaleGraphics;

            if (localeGraphics != null && localeGraphics.ContainsKey(localeCode))
            {
                return localeGraphics[localeCode].TopAttractVideo;
            }

            return null;
        }

        /// <summary>
        /// Gets the topper attract video for the game for the specified locale code
        /// </summary>
        /// <param name="localeCode"></param>
        /// <returns></returns>
        public string GetTopperAttractVideoPathByLocaleCode(string localeCode)
        {
            var localeGraphics = (Dictionary<string, ILocaleGameGraphics>)LocaleGraphics;

            if (localeGraphics != null && localeGraphics.ContainsKey(localeCode))
            {
                return localeGraphics[localeCode].TopperAttractVideo;
            }

            return null;
        }

        /// <summary>
        /// Gets the bottom attract video for the game for the specified locale code
        /// </summary>
        /// <param name="localeCode"></param>
        /// <returns></returns>
        public string GetBottomAttractVideoPathByLocaleCode(string localeCode)
        {
            var localeGraphics = (Dictionary<string, ILocaleGameGraphics>)LocaleGraphics;

            if (localeGraphics != null && localeGraphics.ContainsKey(localeCode))
            {
                return localeGraphics[localeCode].BottomAttractVideo;
            }

            return null;
        }
    }
}