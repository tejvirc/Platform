namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using CommunityToolkit.Mvvm.Input;

    /// <summary>
    ///     TimerInfo has the properties related to timer control HHR pages
    /// </summary>
    public class TimerInfo
    {
        /// <summary>
        ///     Timeout --Maximum time, after that timer would expire
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        ///     IsVisible --To set the visibility of a timer on any HHR page
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        ///     Enable or disable the timer
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     IsQuickPickTextVisible --To make title of timer visible/hidden for Quick-Pick
        /// </summary>
        public bool IsQuickPickTextVisible { get; set; }

        /// <summary>
        ///     IsAutoPickTextVisible --To make title of timer visible/hidden for Auto-Pick
        /// </summary>
        public bool IsAutoPickTextVisible { get; set; }

        /// <summary>
        ///     TimerElapsedCommand --Command to execute after timer expires
        /// </summary>
        public IRelayCommand TimerElapsedCommand { get; set; }

        /// <summary>
        ///     UnitTimeElapsedCommand --Command to execute after unit time elapsed to track the timer in view model.
        /// </summary>

        public IRelayCommand TimerTickCommand { get; set; }
    }
}
