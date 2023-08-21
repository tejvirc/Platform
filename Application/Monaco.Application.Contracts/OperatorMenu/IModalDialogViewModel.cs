namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System.ComponentModel;

    /// <summary>
    ///     A view model representing a modal dialog opened using <see cref="IDialogService" />.
    /// </summary>
    public interface IModalDialogViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets the dialog result value, which is the value that is returned from the
        ///     <see cref="IDialogService.ShowDialog{T}" /> method.
        /// </summary>
        bool? DialogResult { get; }
    }
}