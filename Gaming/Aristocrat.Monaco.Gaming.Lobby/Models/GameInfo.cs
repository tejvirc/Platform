namespace Aristocrat.Monaco.Gaming.Lobby.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Contracts;
using Contracts.Models;
using Kernel;
using ManagedBink;
using Toolkit.Mvvm.Extensions;
using UI.Common.Extensions;
using Size = System.Windows.Size;

public class GameInfo : ObservableObject, IGameInfo, IImageIcon
{
    private const string BinkFileExtension = ".bk2";

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

    private DateTime _installDateTime = DateTime.UtcNow;

    private bool _isNew;
    private string _loadingScreenPath = string.Empty;

    private string _name;

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

    private Size _gameIconSize = Size.Empty;

    public Size GameIconSize
    {
        get
        {
            if (_gameIconSize != Size.Empty || string.IsNullOrEmpty(_imagePath))
            {
                return _gameIconSize;
            }

            if (IsBinkImage)
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

    public bool IsBinkImage => Path.GetExtension(ImagePath) == BinkFileExtension;

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

        set => this.SetProperty(ref _imagePath, value, OnPropertyChanged, nameof(ImagePath), nameof(IsBinkImage));
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
    ///     Gets or sets the attract higlight video path.  This can be data driven
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

        set => this.SetProperty(
            ref _progressiveIndicator,
            value,
            OnPropertyChanged,
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
    public bool HasProgressiveOrBonusValue
    {
        get => _hasProgressiveOrBonusValue;

        set => this.SetProperty(
            ref _hasProgressiveOrBonusValue,
            value,
            OnPropertyChanged,
            nameof(ProgressiveOrBonusValue),
            nameof(HasProgressiveOrBonusValue));
    }

    /// <summary>
    ///     Gets or sets the denomination
    /// </summary>
    public long Denomination
    {
        get => _denom;

        set => SetProperty(ref _denom, value);
    }

    /// <summary>
    ///     The list of denomination buttons that will appear below the extra large game icons
    /// </summary>
    public ObservableCollection<DenomInfo> Denominations { get; } = new();

    public void SetDenominations(IEnumerable<long> denominations)
    {
        Denominations.Clear();
        Denominations.AddRange(denominations.OrderBy(x => x).Select(x => new DenomInfo(x) { IsVisible = true }));
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

    public bool IsSelected
    {
        get => _isSelected;

        set => this.SetProperty(
            ref _isSelected,
            value,
            OnPropertyChanged,
            nameof(IsSelected),
            nameof(IsSelectedWithProgressiveLabel));
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
    ///     Theme position priority key of the game
    /// </summary>
    public string PositionPriorityKey { get; set; }

    /// <summary>
    ///     Gets or sets a property stating whether or not this game requires mechanical reels
    /// </summary>
    public bool RequiresMechanicalReels { get; set; }

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
}
