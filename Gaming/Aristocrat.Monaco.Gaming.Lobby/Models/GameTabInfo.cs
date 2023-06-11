namespace Aristocrat.Monaco.Gaming.Lobby.Models;

using Aristocrat.Monaco.Gaming.Contracts.Models;
using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Toolkit.Mvvm.Extensions;

public class GameTabInfo : ObservableObject
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

        set => this.SetProperty(
            ref _enabled,
            value,
            OnPropertyChanged,
            nameof(Enabled),
            nameof(LabelResourceKey),
            nameof(LabelAnimatedResourcePath),
            nameof(HasAnimatedResource),
            nameof(TabResourceKey));
    }

    public int TabIndex { get; }

    public GameCategory Category { get; }

    public string LabelResourceKey => $"{LabelResourceKeyPrefix}{Category}{DisabledText}";

    public string LabelAnimatedResourcePath => $"{AppDomain.CurrentDomain.BaseDirectory}{ImagesPath}{LabelResourceKey}.gif";

    public bool HasAnimatedResource => File.Exists(LabelAnimatedResourcePath);

    public string TabResourceKey => $"{TabResourceKeyPrefix}{TabIndex + 1}{DisabledText}";

    private string DisabledText => Enabled ? string.Empty : Disabled;
}
