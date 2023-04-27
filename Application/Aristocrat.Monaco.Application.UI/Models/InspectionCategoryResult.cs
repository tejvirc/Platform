namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Contracts.ConfigWizard;
    using Contracts.HardwareDiagnostics;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class InspectionCategoryResult : BaseEntityViewModel
    {
        public const string CheckMark = "\x221A";

        public InspectionCategoryResult(InspectionResultData data)
        {
            Category = Enum.GetName(typeof(HardwareDiagnosticDeviceCategory), data.Category);

            switch (data.Status)
            {
                case InspectionPageStatus.Untested:
                    StatusText = "?";
                    break;
                case InspectionPageStatus.Good:
                    StatusText = CheckMark;
                    break;
                case InspectionPageStatus.Bad:
                    StatusText = "X";
                    break;
            }

            Status = data.Status;
            FirmwareMessage = data.FirmwareVersion;
            FailureMessage = string.Join("; ", data.CombinedTestFailures);

            RaisePropertyChanged(
                nameof(Category),
                nameof(StatusText),
                nameof(Status),
                nameof(FirmwareMessage),
                nameof(FailureMessage));
        }

        public string Category { get; set; }

        public string StatusText { get; set; }

        public InspectionPageStatus Status { get; set; }

        public string FirmwareMessage { get; set; }

        public string FailureMessage { get; set; }
    }
}
