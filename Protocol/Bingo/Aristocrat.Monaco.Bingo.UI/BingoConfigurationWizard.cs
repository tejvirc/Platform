namespace Aristocrat.Monaco.Bingo.UI
{
    using System.Collections.ObjectModel;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Loaders;
    
    public sealed class BingoConfigurationWizard : IComponentWizard
    {
        public BingoConfigurationWizard()
        {
            WizardPages = new Collection<IOperatorMenuPageLoader>
            {
                new BingoHostConfigurationLoader(true),
            };
        }
        
        public Collection<IOperatorMenuPageLoader> WizardPages { get; }
    }
}