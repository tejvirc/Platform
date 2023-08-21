namespace Aristocrat.Monaco.UI.Common
{
    using System.Windows.Threading;

    /// <summary>
    ///     Implements IDispatcherTimer using DispatcherTimer.
    /// </summary>
    public class DispatcherTimerAdapter : DispatcherTimer, ITimer
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DispatcherTimerAdapter" /> class.
        /// </summary>
        public DispatcherTimerAdapter()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DispatcherTimerAdapter" /> class.
        /// </summary>
        /// <param name="priority">The priority of the timer.</param>
        public DispatcherTimerAdapter(DispatcherPriority priority)
            : base(priority)
        {
        }
    }
}