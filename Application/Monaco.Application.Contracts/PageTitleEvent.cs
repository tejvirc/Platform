namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event used to set the page title content.
    /// </summary>
    public class PageTitleEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PageTitleEvent" /> class.
        /// </summary>
        /// <param name="content">The button text.</param>
        [CLSCompliant(false)]
        public PageTitleEvent(string content)
        {
            Content = content;
        }

        /// <summary>Gets page title content.</summary>
        [CLSCompliant(false)]
        public string Content { get; }
    }
}