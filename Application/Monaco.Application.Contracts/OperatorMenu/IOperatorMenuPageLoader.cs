namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System;
    /// <summary>
    /// IOperatorMenuPageLoader
    /// </summary>
    public interface IOperatorMenuPageLoader : IDisposable, IOperatorMenuConfigObject
    {
        /// <summary>
        ///     Gets the label to be displayed for this menu item
        /// </summary>
        string PageName { get; }

        /// <summary>
        ///     Gets or sets whether this page is enabled
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets the UserControl page for this loader
        /// </summary>
        IOperatorMenuPage Page { get; }

        /// <summary>
        ///     Gets the ViewModel for this loader
        /// </summary>
        IOperatorMenuPageViewModel ViewModel { get; }

        /// <summary>
        ///     Sets whether this page is a subpage of another page
        /// </summary>
        bool IsSubPage { get; set; }

        /// <summary>
        ///     Gets whether this page is a multi-page
        /// </summary>
        bool IsMultiPage { get; }

        /// <summary>
        ///     Gets whether this page is a wizard (setup) page
        /// </summary>
        bool IsWizardPage { get; set; }

        /// <summary>
        ///     Determines whether the menu item is visible or hidden 
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Initialize
        /// </summary>
        void Initialize();

        /// <summary>
        /// OnEnabledChanged
        /// </summary>
        event EventHandler OnEnabledChanged;
    }
}
