namespace Aristocrat.Monaco.Hhr.UI
{
    using System.Collections.ObjectModel;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Loaders;

    /// <summary>
    ///     Defines the pages to load for configuration.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class HHRConfigurationWizard : IComponentWizard
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HHRConfigurationWizard" /> class.
        /// </summary>
        /// <remarks>
        ///     Constructs a new instance initializing the WizardPages collection.
        /// </remarks>
        public HHRConfigurationWizard()
        {
            WizardPages = new Collection<IOperatorMenuPageLoader>();
            if (!NetworkConfigPageLoader.IsInstantiated)
            {
                WizardPages.Add(new NetworkConfigPageLoader(true));
            }
            WizardPages.Add(new ServerConfigurationPageViewLoader(true));
        }

        /// <inheritdoc />
        public Collection<IOperatorMenuPageLoader> WizardPages { get; }
    }
}
