namespace Aristocrat.Monaco.Gaming.Presentation.Views;

using System;
using System.Windows;
using System.Windows.Controls;
using Aristocrat.Monaco.Gaming.Presentation.ViewModels;
using Localization.Properties;

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
        double w = IdleTextBlockBlinking.ActualWidth;
        ((BannerViewModel)DataContext).IsScrollingDisplayMode = w > MaximumBlinkingIdleTextWidth;
    }
}