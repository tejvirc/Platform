namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;
    using Application.Contracts;
    using ConfigWizard;
    using Contracts.ConfigWizard;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM.ViewModel;
    using OperatorMenu;

    /// <summary>
    ///     Page to display inspection summary
    /// </summary>
    [CLSCompliant(false)]
    public class InspectionSummaryPageViewModel : InspectionWizardViewModelBase
    {
        private const int DataLinesPerPage = 36;
        private const string TicketType = "text";

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

            var serviceManager = ServiceManager.GetInstance();
            var dateFormat = serviceManager.GetService<IPropertiesManager>().GetValue(
                ApplicationConstants.LocalizationOperatorDateFormat,
                ApplicationConstants.DefaultDateFormat);
            Timestamp = serviceManager.GetService<ITime>().GetFormattedLocationTime(DateTime.Now, $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}");

            base.OnLoaded();
        }

        public string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionSummaryTitle);

        public string Timestamp { get; private set; }

        public ObservableCollection<InspectionCategoryResult> Reports { get; } = new ();

        protected override void SaveChanges()
        {
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            var tickets = new List<Ticket>();

            var pageNum = 1;
            var text = string.Empty;

            foreach (var result in Inspection.Results)
            {
                if (result.Category == HardwareDiagnosticDeviceCategory.Unknown)
                {
                    continue;
                }

                var categoryText = BuildCategoryText(result);

                if (CountLines(text + categoryText) > DataLinesPerPage)
                {
                    tickets.Add(CreateTicket(text, pageNum));

                    pageNum++;
                    text = string.Empty;
                }

                text += categoryText;
            }

            tickets.Add(CreateTicket(text, pageNum));

            return tickets;
        }

        private string BuildCategoryText(InspectionResultData result)
        {
            var text = new StringBuilder();
            text.AppendLine();
            text.AppendLine($"{result.Category}  {(result.Status == InspectionPageStatus.Good ? "OK" : result.Status == InspectionPageStatus.Bad ? "FAIL" : "?")}");

            if (!string.IsNullOrEmpty(result.FirmwareVersion))
            {
                result.FirmwareVersion.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList().ForEach(f => text.AppendLine($"{f}"));
            }

            result.FailureMessages.ToList().ForEach(m => text.AppendLine($" FAIL {m}"));

            return text.ToString();
        }

        private Ticket CreateTicket(string text, int pageNum)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{PageName}  {Timestamp}  p.{pageNum}");
            sb.Append(text);

            return new Ticket
            {
                [TicketConstants.TicketType] = TicketType,
                [TicketConstants.Left] = sb.ToString()
            };
        }

        private int CountLines(string field)
        {
            return field.Split(new [] { Environment.NewLine }, StringSplitOptions.None).Length;
        }
    }

    [CLSCompliant(false)]
    public class InspectionCategoryResult : BaseEntityViewModel
    {
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush YellowBrush = new SolidColorBrush(Colors.Yellow);
        private static readonly SolidColorBrush GreenBrush = new SolidColorBrush(Colors.LightGreen);

        public const string CheckMark = "\x221A";

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
                    StatusText = CheckMark;
                    StatusColor = GreenBrush;
                    break;
                case InspectionPageStatus.Bad:
                    StatusText = "X";
                    StatusColor = RedBrush;
                    break;
            }

            FirmwareMessage = data.FirmwareVersion;
            FailureMessage = string.Join("; ", data.CombinedTestFailures);

            RaisePropertyChanged(
                nameof(Category),
                nameof(StatusText),
                nameof(StatusColor),
                nameof(FirmwareMessage),
                nameof(FailureMessage));
        }

        public string Category { get; set; }

        public string StatusText { get; set; }

        public Brush StatusColor { get; set; }

        public string FirmwareMessage { get; set; }

        public string FailureMessage { get; set; }
    }
}
