namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Application.UI.OperatorMenu;

    [CLSCompliant(false)]
    public class InspectionPageReportFailureViewModel : OperatorMenuSaveViewModelBase
    {
        private string _reportFailureText;

        public InspectionPageReportFailureViewModel()
        {
            //Initialize();
        }

        public string ReportFailureText
        {
            get => _reportFailureText;
            set => SetProperty(ref _reportFailureText, value, nameof(ReportFailureText));
        }

    }
}
