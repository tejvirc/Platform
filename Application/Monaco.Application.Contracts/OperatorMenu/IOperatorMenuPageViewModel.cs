namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System;
    using System.ComponentModel;
    /// <summary>
    ///     The interface to expose information about an operator menu page's view model
    /// </summary>
    public interface IOperatorMenuPageViewModel : IDisposable, INotifyPropertyChanged, IOperatorMenuConfigObject
    {
        /// <summary>
        ///     True if the page is currently loading data
        /// </summary>
        bool IsLoadingData { get; }

        /// <summary>
        ///     True if the page is currently load.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Data Empty
        /// Individual operator menu pages can override this property to indicate when they
        /// do not have any data to print.
        /// </summary>
        bool DataEmpty { get; }

        /// <summary>
        /// MainPrintButtonEnabled contains the current state
        /// of the main print button including access rule modifications
        /// </summary>
        bool MainPrintButtonEnabled { get; }

        /// <summary>
        ///     Returns true if calibration is allowed on this page
        /// </summary>
        bool CanCalibrateTouchScreens { get; }

        /// <summary>
        /// Whether the popup is currently open
        /// </summary>
        bool PopupOpen { get; set; }
    }
}
