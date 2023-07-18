namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.ConfigWizard;
    using Contracts.HardwareDiagnostics;

    [CLSCompliant(false)]
    public class InspectionCategoryResult : BaseObservableObject
    {
        public const string BadMark = "X";
        public const string CheckMark = "\x221A";
        public const string QuestionMark = "?";

        public InspectionCategoryResult(InspectionResultData data)
        {
            Category = Enum.GetName(typeof(HardwareDiagnosticDeviceCategory), data.Category);

            switch (data.Status)
            {
                case InspectionPageStatus.Untested:
                    StatusText = QuestionMark;
                    break;
                case InspectionPageStatus.Good:
                    StatusText = CheckMark;
                    break;
                case InspectionPageStatus.Bad:
                    StatusText = BadMark;
                    break;
            }

            Status = data.Status;
            FirmwareMessage = data.CombinedFirmwareVersions(Environment.NewLine);
            FailureMessage = data.CombinedTestFailures(Environment.NewLine);

            OnPropertyChanged(
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
