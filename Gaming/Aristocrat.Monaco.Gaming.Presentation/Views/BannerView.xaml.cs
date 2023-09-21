namespace Aristocrat.Monaco.Gaming.Presentation.Views;

using System.Windows;
using System.Windows.Controls;
using ViewModels;

/// <summary>
///     Interaction logic for BannerView.xaml
/// </summary>
public partial class BannerView
{
    private const double MaximumBlinkingIdleTextWidth = 1000;

    public BannerView()
    {
        InitializeComponent();
    }

    private void IdleTextBoxInvisible_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        // Pass localized text down to state
        ((BannerViewModel)DataContext).JurisdictionIdleText = IdleTextBoxInvisible.Text;
    }

    private void IdleTextBlockBlinking_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var w = IdleTextBlockBlinking.ActualWidth;
        ((BannerViewModel)DataContext).IsScrollingDisplayMode = w > MaximumBlinkingIdleTextWidth;
    }
}