namespace Aristocrat.Monaco.Gaming.VideoLottery.ScreenSaver
{
    using Kernel;

    public class ScreenSaverVisibilityEvent : BaseEvent
    {
        public ScreenSaverVisibilityEvent(bool show)
        {
            Show = show;
        }

        public bool Show { get; }
    }
}