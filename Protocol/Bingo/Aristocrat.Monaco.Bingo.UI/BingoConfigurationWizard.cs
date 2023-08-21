namespace Aristocrat.Monaco.Bingo.UI
{
    using System.Collections.ObjectModel;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Loaders;
    
    public sealed class BingoConfigurationWizard : IComponentWizard
    {
        public BingoConfigurationWizard()
        {
            WizardPages = new Collection<IOperatorMenuPageLoader>();
            if (!NetworkConfigPageLoader.IsInstantiated)
            {
                WizardPages.Add(new NetworkConfigPageLoader(true));
            }
            WizardPages.Add(new BingoHostConfigurationLoader(true));
        }

        public Collection<IOperatorMenuPageLoader> WizardPages { get; }
    }
}