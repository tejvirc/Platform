namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using System;
    using Contracts.ConfigWizard;
    using Kernel;

    [CLSCompliant(false)]
    public class InspectionWizardViewModelBase : ConfigWizardViewModelBase
    {
        public InspectionWizardViewModelBase(bool isWizard)
            : this(ServiceManager.GetInstance().TryGetService<IInspectionService>(),
                  isWizard)
        { }

        public InspectionWizardViewModelBase(
            IInspectionService inspectionResults,
            bool isWizard) : base(isWizard)
        {
            Inspection = inspectionResults;
        }

        public IInspectionService Inspection { get; }

        protected override void SaveChanges()
        {
        }
    }
}
