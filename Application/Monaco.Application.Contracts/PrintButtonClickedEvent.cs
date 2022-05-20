namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event for when a Print button is clicked.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is posted when the print button on the MenuSelectionWindow is clicked.
    ///     </para>
    ///     <para>
    ///         The event should be handled by any operator menu components which need to print
    ///         specific page content.
    ///     </para>
    /// </remarks>
    public class PrintButtonClickedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintButtonClickedEvent" /> class.
        /// </summary>
        /// <param name="cancel">Indicates whether or not this a cancel print event.</param>
        [CLSCompliant(false)]
        public PrintButtonClickedEvent(bool cancel = false)
        {
            Cancel = cancel;
        }

        /// <summary>Gets a value indicating whether or not this is a cancel print event.</summary>
        [CLSCompliant(false)]
        public bool Cancel { get; }
    }
}