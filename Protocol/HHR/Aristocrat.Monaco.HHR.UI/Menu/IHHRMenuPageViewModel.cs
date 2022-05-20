namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;

    /// <summary>
    /// The interface to expose information about an Hhr menu page view model
    /// </summary>
    public interface IHhrMenuPageViewModel : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        ///     Button-Command present on Hhr page
        /// </summary>
        List<HhrPageCommand> Commands { get; }

        /// <summary>
        ///     Button-Command The Event that would be raised on click  Hhr page button
        /// </summary>
        event EventHandler<HHRCommandEventArgs> HhrButtonClicked;

        /// <summary>
        ///     Event to show placard
        /// </summary>
        event EventHandler<PlacardEventArgs> PlacardEvent;

        /// <summary>
        ///     To initialize the View model
        /// </summary>
        Task Init(Command command);

        /// <summary>
        ///     To reset the View model to it's original state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Flag to show the footer texgt
        /// </summary>
        bool ShowFooterText { get; set; }

        /// <summary>
        /// Keeps time in timer control in the view pages that require it
        /// </summary>
        TimerInfo TimerInfo { get; set; }
    }
}