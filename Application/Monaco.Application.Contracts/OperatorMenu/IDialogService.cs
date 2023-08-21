namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Kernel;

    /// <summary>
    ///     DialogButton
    /// </summary>
    [Flags]
    public enum DialogButton
    {
        /// <summary>None</summary>
        None = 0,

        /// <summary>Save</summary>
        Save = 1,

        /// <summary>Cancel</summary>
        Cancel = 2
    }

    /// <summary>
    ///     Provides a mechanism to show a modal dialog in a WPF view. Due to the
    ///     dialogs being modal, only ONE dialog may be displayed at any given time.
    /// </summary>
    public interface IDialogService : IService
    {
        /// <summary>
        ///     This flag will be <c>true</c> while a modal dialog is open. If no dialog is open,
        ///     will return <c>false</c>, indicating that a dialog may be opened.
        /// </summary>
        public bool IsDialogOpen { get; }

        /// <summary>
        ///     Use when you have a custom view(T) and you either want the
        ///     default settings or need full control of the dialog setup
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ownerViewModel"></param>
        /// <param name="viewModel"></param>
        /// <param name="windowTitle"></param>
        /// <param name="buttons"></param>
        /// <param name="customButtonText"></param>
        /// <param name="windowInfoText"></param>
        /// <returns>The dialog result</returns>
        bool? ShowDialog<T>(
            INotifyPropertyChanged ownerViewModel,
            IModalDialogSaveViewModel viewModel,
            string windowTitle,
            DialogButton buttons = DialogButton.Save | DialogButton.Cancel,
            IEnumerable<IDialogButtonCustomTextItem> customButtonText = null,
            string windowInfoText = null)
            where T : IOperatorMenuPage;

        /// <summary>
        ///     Use when you want the default view (text only) and full customization
        ///     of buttons and button text
        /// </summary>
        /// <param name="ownerViewModel"></param>
        /// <param name="windowTitle"></param>
        /// <param name="buttons"></param>
        /// <param name="customText"></param>
        /// <param name="windowInfoText"></param>
        /// <returns>The dialog result. If null is returned, the dialog failed to be displayed.</returns>
        bool? ShowDialog(
            INotifyPropertyChanged ownerViewModel,
            string windowTitle,
            DialogButton buttons = DialogButton.Save | DialogButton.Cancel,
            IEnumerable<IDialogButtonCustomTextItem> customText = null,
            string windowInfoText = null);

        /// <summary>
        ///     Use when you want the default view with Yes and No Buttons
        /// </summary>
        /// <param name="ownerViewModel"></param>
        /// <param name="windowTitle"></param>
        /// <param name="windowInfoText"></param>
        /// <returns>The dialog result. If null is returned, the dialog failed to be displayed.</returns>
        bool? ShowYesNoDialog(INotifyPropertyChanged ownerViewModel, string windowTitle, string windowInfoText = null);

        /// <summary>
        ///     Use when you have a custom view (T) and you
        ///     want only a Cancel Button that says "Close"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ownerViewModel"></param>
        /// <param name="viewModel"></param>
        /// <param name="windowTitle"></param>
        /// <returns>The dialog result. If null is returned, the dialog failed to be displayed.</returns>
        bool? ShowInfoDialog<T>(
            INotifyPropertyChanged ownerViewModel,
            IModalDialogSaveViewModel viewModel,
            string windowTitle)
            where T : IOperatorMenuPage;

        /// <summary>
        ///     Provide a way to dismiss an opened dialog window programmatically
        ///     Use when need to programmatically dismiss a dialog without clicking 'Cancel' or 'Close' button
        /// </summary>
        /// <returns>True if opened dialog is dismissed, false otherwise</returns>
        bool DismissOpenedDialog();
    }

    /// <summary>
    ///     IDialogButtonCustomTextItem
    /// </summary>
    public interface IDialogButtonCustomTextItem
    {
        /// <summary>Button the Text is for</summary>
        DialogButton Button { get; set; }

        /// <summary>Text</summary>
        string Text { get; set; }
    }
}