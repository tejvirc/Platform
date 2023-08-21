namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    using OperatorMenu;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     An interface to expose all information about a wizard component.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         In order for a wizard component to be displayed for configuration or just
    ///         for prompting, you have to implement this interface and configure your component
    ///         into the mono-addin configuration file as an extension node so that the component
    ///         loading all wizards can recognize it.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     You create a wizard like below:
    ///     <code>
    ///       public class YourWizard : IComponentWizard
    ///       {
    ///         public string PageName { get { return "Test Item"; } }
    ///         public Collection&lt;Page&gt; WizardPages
    ///         {
    ///           get
    ///           {
    ///             Collection&lt;Page&gt; pages = new Collection&lt;Page&gt;();
    ///             pages.Add(new TheFirstPage());
    ///             pages.Add(new TheSecondPage());
    ///             return pages;
    ///           }
    ///         }
    ///       }
    ///     </code>
    ///     Configure it into a mono-addin file like:
    ///     <code>
    ///       &lt;Addin id="YourWizard" namespace="Client12Addins" version="1.0"&gt;
    ///         &lt;Runtime&gt;
    ///           &lt;Import assembly="YourWizard.dll" /&gt;
    ///         &lt;/Runtime&gt;
    ///         &lt;Dependencies&gt;
    ///           &lt;Addin id="ConfigWizard" version="1.0" /&gt;
    ///         &lt;/Dependencies&gt;
    ///         &lt;Extension path="/Application/Config/Wizards"&gt;
    ///           &lt;Wizard type="YourWizard" /&gt;
    ///         &lt;/Extension&gt;
    ///       &lt;/Addin&gt;
    ///     </code>
    /// </example>
    public interface IComponentWizard
    {
        /// <summary>
        ///     Gets the list of Config pages for this wizard.
        /// </summary>
        /// <returns>A collection of pages in the order they should be displayed.</returns>
        Collection<IOperatorMenuPageLoader> WizardPages { get; }
    }
}
