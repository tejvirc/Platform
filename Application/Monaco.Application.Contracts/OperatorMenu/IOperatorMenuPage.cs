namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    using System;
    /// <summary>
    ///     The interface to expose information about an item displayed on the operator menu screen
    /// </summary>
    /// <remarms>
    ///     <para>
    ///         An item on the operator menu screen may contain more than one ordered pages. The first page
    ///         in the list managed by the operator menu item will be displayed when the label is clicked.
    ///     </para>
    ///     <para>
    ///         Implement this interface to add a menu item to the top level operator menu screen.
    ///         Your implementation must be configured as an add-in to be discovered and loaded.
    ///     </para>
    /// </remarms>
    /// <example>
    ///     <code>
    ///       // Create a UserControl and assign the PageName that should appear in the menu item list
    ///       // Override the virtual methods in the code-behind if needed
    ///       &lt;OperatorMenuPage x:Class="Aristocrat.Monaco.Application.UI.Views.YourPage"/&gt;
    ///       public partial class YourPage
    ///       {
    ///       }
    ///    </code>
    ///     Now you can configure it to a mono-addin file:
    ///     <code>
    ///      &lt;Addin id="YourOperatorMenuItem" namespace="Client12Addins" version="1.0"&gt;
    ///        &lt;Runtime&gt;
    ///          &lt;Import assembly="YourOperatorMenuItem.dll" /&gt;
    ///        &lt;/Runtime&gt;
    ///        &lt;Dependencies&gt;
    ///          &lt;Addin id="OperatorMenu" version="1.0" /&gt;
    ///        &lt;/Dependencies&gt;
    ///        &lt;Extension path="/Application/OperatorMenu/MainMenu"&gt;
    ///          &lt;OperatorMenu type="YourOperatorMenuItem" /&gt;
    ///        &lt;/Extension&gt;
    ///      &lt;/Addin&gt;
    ///    </code>
    /// </example>
    public interface IOperatorMenuPage : IDisposable
    {
    }
}
