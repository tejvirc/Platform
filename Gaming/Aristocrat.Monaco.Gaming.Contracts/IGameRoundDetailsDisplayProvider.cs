namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.ComponentModel;
    using Application.Contracts.OperatorMenu;
    using Kernel;

    /// <summary>
    ///     Interface for interacting with the means to display details for game rounds.
    /// </summary>
    public interface IGameRoundDetailsDisplayProvider : IService
    {
        /// <summary>
        ///     Displays the game round details associated with the given centralTransactionId in a pop-up window.
        /// </summary>
        /// <param name="ownerViewModel">The window in which the details will appear as a pop-up.</param>
        /// <param name="dialogService">The <see cref="IDialogService"/> used to hold the View Model display.</param>
        /// <param name="windowTitle">The title of the game round details pop-up window.</param>
        /// <param name="centralTransactionId">The central transaction ID for the selected game round.</param>
        void Display(INotifyPropertyChanged ownerViewModel, IDialogService dialogService, string windowTitle, long centralTransactionId);
    }
}
