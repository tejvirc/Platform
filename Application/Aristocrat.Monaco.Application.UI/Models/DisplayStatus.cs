namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This is used to display the display status as connected or disconnected./>. 
    ///     This is created by the display page viewmodel implementation for use in displays page UIs.
    /// </summary>
    [CLSCompliant(false)]
    public class DisplayStatus : BaseViewModel
    {
        private bool _disconnected;
        private string _status;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayStatus"/> class.
        /// </summary>
        /// <param name="name">The name of the display</param>
        /// <param name="id">The id of the display or touch screen</param>
        /// <param name="disconnected">Whether or not the display or touch screen is disconnected</param>
        public DisplayStatus(string name, string id, bool disconnected)
        {
            Name = name;
            Id = id;
            Disconnected = disconnected;
        }

        /// <summary>
        ///     The name of the display
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The id of the display or touch screen
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Indicates whether or not the display or touch screen is disconnected
        /// </summary>
        public bool Disconnected
        {
            get => _disconnected;
            set
            {
                _disconnected = value;
                Status = _disconnected ? string.IsNullOrEmpty(Name) ? string.Empty : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnected) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConnectedText);
            }
        }

        /// <summary>
        ///     The status of the display
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }
    }
}
