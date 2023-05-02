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
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Default resource key for denomination button panel
        private const string DenomPanelDefaultKey = "DenominationPanel";

        private string _attractHighlightVideoPath = string.Empty;
        private string _bottomAttractVideoPath = string.Empty;
        private long _denom = 1;
        private string _betOption = string.Empty;
        private long _filteredDenom = 1;
        private string _dllPath;
        private bool _enabled;
        private GameType _gameType = GameType.Slot;
        private string _gameSubtype;
        private GameCategory _category;
        private GameSubCategory _subCategory;
        private bool _isSelected;
        private bool _hasProgressiveOrBonusValue;
        private string _imagePath;
        private string _denomButtonPath;
        private string _denomButtonResourceKey;
        private string _denomPanelPath;
        private DenominationInfoViewModel _selectedDenomination;

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

            set
            {
                if (_dllPath != value)
                {
                    _dllPath = value;
                    RaisePropertyChanged(nameof(DllPath));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the game name
        /// </summary>
        public string Name
        {
            get => _name;

            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

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

        public bool ImageIsBink => ImagePath.EndsWith(".bk2");

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

            set
            {
                if (_topAttractVideoPath != value)
                {
                    _topAttractVideoPath = value;
                    RaisePropertyChanged(nameof(TopAttractVideoPath));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the topper screen attract video path
        /// </summary>
        public string TopperAttractVideoPath
        {
            get => _topperAttractVideoPath;

            set
            {
                if (_topperAttractVideoPath != value)
                {
                    _topperAttractVideoPath = value;
                    RaisePropertyChanged(nameof(TopperAttractVideoPath));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the bottom screen attract video path
        /// </summary>
        public string BottomAttractVideoPath
        {
            get => _bottomAttractVideoPath;

            set
            {
                if (_bottomAttractVideoPath != value)
                {
                    _bottomAttractVideoPath = value;
                    RaisePropertyChanged(nameof(BottomAttractVideoPath));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the image to show when a game is loading
        /// </summary>
        public string LoadingScreenPath
        {
            get => _loadingScreenPath;

            set
            {
                if (_loadingScreenPath != value)
                {
                    _loadingScreenPath = value;
                    RaisePropertyChanged(nameof(LoadingScreenPath));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the attract higlight video path.  This can be data driven
        ///     and vary per game; however, right now the lobby sets it based on a global setting.
        /// </summary>
        public string AttractHighlightVideoPath
        {
            get => _attractHighlightVideoPath;

            set
            {
                if (_attractHighlightVideoPath != value)
                {
                    _attractHighlightVideoPath = value;
                    RaisePropertyChanged(nameof(AttractHighlightVideoPath));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the game progressive value
        /// </summary>
        public string ProgressiveOrBonusValue
        {
            get => _progressiveOrBonusValue;

            set
            {
                if (_progressiveOrBonusValue != value)
                {
                    _progressiveOrBonusValue = value;
                    RaisePropertyChanged(nameof(ProgressiveOrBonusValue));
                }
            }
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
            set => SetProperty(ref _progressiveErrorVisible, value, nameof(ProgressiveErrorVisible));
        }

        public string ProgressiveIndicatorText
        {
            get => _progressiveIndicatorText;
            set => SetProperty(ref _progressiveIndicatorText, value, nameof(ProgressiveIndicatorText));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the game has a progressive bonus
        /// </summary>
        public bool HasProgressiveOrBonusValue
        {
            get => _hasProgressiveOrBonusValue;

            set
            {
                if (_hasProgressiveOrBonusValue != value)
                {
                    _hasProgressiveOrBonusValue = value;
                    RaisePropertyChanged(nameof(ProgressiveOrBonusValue));
                    RaisePropertyChanged(nameof(HasProgressiveOrBonusValue));
                }
            }
        }

        private string ThemeKey => $"{ThemeId}_{GameId}";

        public string DenomButtonPath
        {
            get => _denomButtonPath;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || _denomButtonPath == value)
                {
                    return;
                }

                var key = $"{ThemeKey}_Button";
                if (Application.Current.Resources.Contains(key))
                {
                    return;
                }

                _denomButtonPath = value;

                try
                {
                    // Add the bitmap to the resource dictionary so it can be accessed by key with ImageHelper
                    var uri = new Uri(_denomButtonPath, UriKind.Absolute);
                    Application.Current.Resources.Add(key, new BitmapImage(uri));
                    _denomButtonResourceKey = key;

                    foreach (var denom in Denominations)
                    {
                        denom.DenomButtonSingleOffOverride = _denomButtonResourceKey;
                    }

                    Logger.Debug($"{_denomButtonResourceKey} added to resources for custom denom button at path {_denomButtonPath}");
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

                var key = $"{ThemeKey}_Panel";
                if (Application.Current.Resources.Contains(key))
                {
                    return;
                }

                _denomPanelPath = value;

                try
                {
                    // Add the bitmap to the resource dictionary so it can be accessed by key with ImageHelper
                    var uri = new Uri(_denomPanelPath, UriKind.Absolute);
                    Application.Current.Resources.Add(key, new BitmapImage(uri));
                    DenomPanelKey = key;
                    Logger.Debug($"{DenomPanelKey} added to resources for custom denom panel at path {_denomPanelPath}");
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

            set
            {
                if (_denom != value)
                {
                    _denom = value;
                    RaisePropertyChanged(nameof(Denomination));
                }
            }
        }

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

            set
            {
                if (_betOption != value)
                {
                    _betOption = value;
                    RaisePropertyChanged(nameof(BetOption));
                }
            }
        }

        /// <summary>
        /// Gets or sets the denomination for filtering
        /// </summary>
        public long FilteredDenomination
        {
            get => _filteredDenom;

            set
            {
                if (_filteredDenom != value)
                {
                    _filteredDenom = value;
                    RaisePropertyChanged(nameof(FilteredDenomination));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the game type
        /// </summary>
        public GameType GameType
        {
            get => _gameType;

            set
            {
                if (_gameType != value)
                {
                    _gameType = value;
                    RaisePropertyChanged(nameof(GameType));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the game type
        /// </summary>
        public string GameSubtype
        {
            get => _gameSubtype;

            set
            {
                if (_gameSubtype != value)
                {
                    _gameSubtype = value;
                    RaisePropertyChanged(nameof(GameSubtype));
                }
            }
        }

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

            set
            {
                if (_platinumSeries != value)
                {
                    _platinumSeries = value;
                    RaisePropertyChanged(nameof(PlatinumSeries));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the date created
        /// </summary>
        public DateTime InstallDateTime
        {
            get => _installDateTime;

            set
            {
                if (_installDateTime != value)
                {
                    _installDateTime = value;
                    RaisePropertyChanged(nameof(InstallDateTime));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the game is new or not
        /// </summary>
        public bool IsNew
        {
            get => _isNew;

            set
            {
                if (_isNew != value)
                {
                    _isNew = value;
                    RaisePropertyChanged(nameof(IsNew));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the game is enabled or not
        /// </summary>
        public bool Enabled
        {
            get => _enabled;

            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    RaisePropertyChanged(nameof(Enabled));
                }
            }
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

            set
            {
                if (_category != value)
                {
                    _category = value;
                    RaisePropertyChanged(nameof(Category));
                }
            }
        }

        /// <summary>
        ///     Holds the game subcategory
        /// </summary>
        public GameSubCategory SubCategory
        {
            get => _subCategory;

            set
            {
                if (_subCategory != value)
                {
                    _subCategory = value;
                    RaisePropertyChanged(nameof(SubCategory));
                }
            }
        }

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
        ///     Select the appropriate for the Locale Graphics
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