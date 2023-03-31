namespace Aristocrat.Monaco.Gaming.Lobby.Models;

using Application.Contracts.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using Toolkit.Mvvm.Extensions;

public class DenomInfo : ObservableObject
{
    private bool _isSelected;
    private bool _isVisible;
    private const string DenominationBackOff = "DenominationBackOff";
    private const string DenominationBackOn = "DenominationBackOn";
    private const string DenominationBackOff2 = "DenominationBackOff2";
    private const string DenominationBackOn2 = "DenominationBackOn2";

    public DenomInfo(long denomination)
    {
        Denomination = denomination;
        DenomText = Denomination.MillicentsToCents().FormattedDenomString();
    }

    public long Denomination { get; }

    public bool IsSelected
    {
        get => _isSelected;

        set => this.SetProperty(ref _isSelected, value, OnPropertyChanged, nameof(IsSelected), nameof(DenomText), nameof(DenomBackground));
    }

    public bool IsVisible
    {
        get => _isVisible;

        set => SetProperty(ref _isVisible, value);
    }

    public string DenomBackground => IsSelected ? DenominationBackOn : DenominationBackOff;

    public string DenomBackground2 => IsSelected ? DenominationBackOn2 : DenominationBackOff2;

    public string DenomText { get; set; }
}
