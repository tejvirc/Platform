namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    /// <summary>
    /// IModalDialogSaveViewModel
    /// </summary>
    public interface IModalDialogSaveViewModel : IModalDialogViewModel
    {
        /// <summary>
        /// HasChanges
        /// </summary>
        /// <returns></returns>
        bool HasChanges();

        /// <summary>
        /// Save
        /// </summary>
        /// <returns></returns>
        void Save();

        /// <summary>
        /// Cancel
        /// </summary>
        void Cancel();

        /// <summary>
        /// CanSave
        /// </summary>
        bool CanSave { get; }
    }
}
