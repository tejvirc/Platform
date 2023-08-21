namespace Aristocrat.Monaco.UI.Common.Events
{
    using System;

    /// <summary>
    ///     Scroll request event args.
    /// </summary>
    public class ScrollRequestEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScrollRequestEventArgs" /> class.
        /// </summary>
        /// <param name="scrollType">Scroll type.</param>
        public ScrollRequestEventArgs(ScrollType scrollType)
        {
            ScrollType = scrollType;
        }

        /// <summary>
        ///     Gets the type of scroll.
        /// </summary>
        public ScrollType ScrollType { get; }
    }
}