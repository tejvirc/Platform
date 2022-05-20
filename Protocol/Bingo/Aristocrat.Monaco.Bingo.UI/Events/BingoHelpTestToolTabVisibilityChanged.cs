namespace Aristocrat.Monaco.Bingo.UI.Events
{
    using Kernel;

    public class BingoHelpTestToolTabVisibilityChanged : BaseEvent
    {
        public BingoHelpTestToolTabVisibilityChanged(bool visible)
        {
            Visible = visible;
        }

        public bool Visible { get; }
    }
}
