namespace Aristocrat.Monaco.UI.Common
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using MahApps.Metro.Controls;

    /// <summary>
    ///     A validation helper.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        ///     Method to clear Validation errors on a page
        /// </summary>
        /// <param name="obj">Dependency object whose validation is to be cleared</param>
        /// <remarks>
        ///     In WPF when items on the tab get unloaded from the visual tree the fact that they were marked invalid
        ///     is lost. When a validation error happens, the UI responds to an event in the validation stack
        ///     and marks the item invalid. This marking doesn't get re-evaluated when the item comes back into the visual
        ///     tree unless the binding is also re-evaluated which doesn't occur when a user clicks on a tab item.
        /// </remarks>
        public static void ClearInvalid(DependencyObject obj)
        {
            if (obj is null)
            {
                return;
            }

            foreach (var error in Validation.GetErrors(obj))
            {
                Validation.ClearInvalid((BindingExpressionBase)error.BindingInError);
            }

            foreach (var childObj in obj.GetChildObjects())
            {
                ClearInvalid(childObj);
            }
        }
    }
}
