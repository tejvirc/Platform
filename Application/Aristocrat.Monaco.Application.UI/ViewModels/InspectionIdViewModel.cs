namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Kernel;
    using Kernel.Contracts;

    [CLSCompliant(false)]
    public class InspectionIdViewModel : BaseViewModel
    {
        public InspectionIdViewModel()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public InspectionIdViewModel(IPropertiesManager properties)
        {
            NameAndVersion = (string)properties.GetProperty(KernelConstants.InspectionNameAndVersion, "Unknown v0.0.0");
        }

        public string NameAndVersion { get; }
    }
}
