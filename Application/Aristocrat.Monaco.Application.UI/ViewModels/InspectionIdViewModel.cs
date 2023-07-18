namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;
    using Kernel;
    using Kernel.Contracts;

    [CLSCompliant(false)]
    public class InspectionIdViewModel : BaseObservableObject
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
