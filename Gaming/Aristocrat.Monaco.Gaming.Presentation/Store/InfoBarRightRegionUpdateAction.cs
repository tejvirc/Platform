namespace Aristocrat.Monaco.Gaming.Presentation.Store
{
    using Gaming.Contracts.InfoBar;

    public record InfoBarRightRegionUpdateAction
    {
        public InfoBarRightRegionUpdateAction(
            string text,
            double duration,
            InfoBarColor textColor)
        {
            Text = text;
            Duration = duration;
            TextColor = textColor;
        }
        public string Text { get; init; }

        public double Duration { get; init; }

        public InfoBarColor TextColor { get; init; }
    }
}
