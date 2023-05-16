namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using ConfigWizard;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Models;
    using Monaco.Localization.Properties;
    using OperatorMenu;

    /// <summary>
    ///     Page to display inspection summary
    /// </summary>
    [CLSCompliant(false)]
    public class InspectionSummaryPageViewModel : InspectionWizardViewModelBase
    {
        private const int DataLinesPerPage = 36;
        private const string TicketType = "text";
        private const string BadMark = "X";
        private const string OkMark = "OK";
        private const string QuestionMark = "?";

        private string _qrCodeText = string.Empty;
        private string _timeStamp;
        private string _orderNumber;
        private string _inspectorInitials;

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
            var properties = serviceManager.GetService<IPropertiesManager>();
            var dateFormat = properties.GetValue(
                ApplicationConstants.LocalizationOperatorDateFormat,
                ApplicationConstants.DefaultDateFormat);
            OrderNumber = $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OrderNumberLabel)}: {properties.GetValue(ApplicationConstants.OrderNumber, string.Empty)}";
            InspectorInitials = $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectorLabel)}: {properties.GetValue(ApplicationConstants.InspectorInitials, string.Empty)}";
            Timestamp = serviceManager.GetService<ITime>().GetFormattedLocationTime(DateTime.Now, $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}");
            QrCodeText = QrEncodedText();

            base.OnLoaded();
        }

        public string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionSummaryTitle);

        public string Timestamp
        {
            get => _timeStamp;
            set => SetProperty(ref _timeStamp, value, nameof(Timestamp));
        }

        public string OrderNumber
        {
            get => _orderNumber;
            set => SetProperty(ref _orderNumber, value, nameof(OrderNumber));
        }

        public string InspectorInitials
        {
            get => _inspectorInitials;
            set => SetProperty(ref _inspectorInitials, value, nameof(InspectorInitials));
        }

        public ObservableCollection<InspectionCategoryResult> Reports { get; } = new ();

        public string QrCodeText
        {
            get => _qrCodeText;
            set => SetProperty(ref _qrCodeText, value, nameof(QrCodeText));
        }

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

                var categoryText = BuildTicketCategoryText(result);

                if (CountLines(text + categoryText) > DataLinesPerPage)
                {
                    tickets.Add(CreateTicket(text, pageNum));

                    pageNum++;
                    text = string.Empty;
                }

                text += categoryText;
            }

            if (!string.IsNullOrEmpty(text))
            {
                tickets.Add(CreateTicket(text, pageNum));
            }

            return tickets;
        }

        private string BuildTicketCategoryText(InspectionResultData result)
        {
            var text = new StringBuilder();
            text.AppendLine();
            text.AppendLine($"{result.Category}  {(result.Status == InspectionPageStatus.Good ? "OK" : result.Status == InspectionPageStatus.Bad ? "FAIL" : "?")}");

            result.FirmwareVersions.Distinct().ToList().ForEach(f => text.AppendLine($"{f}"));

            result.FailureMessages.Distinct().ToList().ForEach(m => text.AppendLine($" FAIL {m}"));

            return text.ToString();
        }

        private Ticket CreateTicket(string text, int pageNum)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{OrderNumber}  {Timestamp}  {InspectorInitials}  p.{pageNum}");
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

        private string QrEncodedText()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"|{OrderNumber}|{Timestamp}|{InspectorInitials}|");

            foreach (var result in Inspection.Results)
            {
                if (result.Category == HardwareDiagnosticDeviceCategory.Unknown)
                {
                    continue;
                }

                var status = GetStatusCode(result);
                var firmwares = result.CombinedFirmwareVersions("; ");
                var failures = result.CombinedTestFailures("; ");
                sb.AppendLine($"|{result.Category}|{status}|{firmwares}|{failures}|");
            }

            return sb.ToString();
        }

        private string GetStatusCode(InspectionResultData inspectionResult)
        {
            switch (inspectionResult.Status)
            {
                case InspectionPageStatus.Untested:
                    if (inspectionResult.Category == HardwareDiagnosticDeviceCategory.Machine)
                    {
                        return string.Empty;
                    }
                    return QuestionMark;
                case InspectionPageStatus.Good:
                    return OkMark;
                case InspectionPageStatus.Bad:
                    return BadMark;
                default:
                    return string.Empty;
            }
        }
    }
}
