namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using System;
    public class PlacardEventArgs : EventArgs
    {
        /// <summary>
        ///     Placard if CallAttendant/TimerExpire
        /// </summary>
        public Placard Placard { get; }

        /// <summary>
        ///     Function to execute after time-bound placard is removed
        /// </summary>
        public Action ExitAction { get; }

        /// <summary>
        ///     Time for which Placard should flash on main screen.
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        ///     True if placard should be visible, false if not
        /// </summary>
        public bool IsVisible { get; }

        public PlacardEventArgs(Placard placard, bool isVisible, int timeout = 0, Action exitAction = null)
        {
            IsVisible = isVisible;
            Placard = placard;
            ExitAction = exitAction;
            Timeout = timeout;
        }
    }
}