namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Media;
    using CommunityToolkit.Mvvm.ComponentModel;
    using ConfigWizard;
    using Contracts.ConfigWizard;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.ViewModels;

    /// <summary>
    ///     Page to display inspection summary
    /// </summary>
    [CLSCompliant(false)]
    public class InspectionSummaryPageViewModel : InspectionWizardViewModelBase
    {
        public InspectionSummaryPageViewModel() : base(true)
        {
        }

        protected override void OnLoaded()
        {
            Reports.Clear();
            foreach (var result in Inspection.Results)
            {
                if (result.Category == HardwareDiagnosticDeviceCategory.Unknown)
                {
                    continue;
                }

                Reports.Add(new InspectionCategoryResult(result));
            }

            base.OnLoaded();
        }

        public string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionSummaryTitle);

        public DateTime DateNow => DateTime.Now;

        public ObservableCollection<InspectionCategoryResult> Reports { get; } = new ();

        protected override void SaveChanges()
        {
        }
    }

    [CLSCompliant(false)]
    public class InspectionCategoryResult : ObservableObject
    {
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush YellowBrush = new SolidColorBrush(Colors.Yellow);
        private static readonly SolidColorBrush GreenBrush = new SolidColorBrush(Colors.LightGreen);

        public InspectionCategoryResult(InspectionResultData data)
        {
            Category = Enum.GetName(typeof(HardwareDiagnosticDeviceCategory), data.Category);

            switch (data.Status)
            {
                case InspectionPageStatus.Untested:
                    StatusText = "?";
                    StatusColor = YellowBrush;
                    break;
                case InspectionPageStatus.Good:
                    StatusText = "\x221A"; // check mark
                    StatusColor = GreenBrush;
                    break;
                case InspectionPageStatus.Bad:
                    StatusText = "X";
                    StatusColor = RedBrush;
                    break;
            }

            FirmwareMessage = data.FirmwareVersion;
            FailureMessage = string.Join("; ", data.CombinedTestFailures);

            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(FirmwareMessage));
            OnPropertyChanged(nameof(FailureMessage));
        }

        public string Category { get; set; }

        public string StatusText { get; set; }

        public Brush StatusColor { get; set; }

        public string FirmwareMessage { get; set; }

        public string FailureMessage { get; set; }
    }
}
