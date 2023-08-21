namespace Aristocrat.Monaco.Application.Tickets
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.MeterPage;
    using Kernel;
    using Monaco.Localization.Properties;

    public class MainAccountingMetersTicket : AuditTicket
    {
        private const string MetersExtensionPath = "/Application/OperatorMenu/DisplayMeters";

        protected List<MeterNode> MeterNodes = new List<MeterNode>();

        public MainAccountingMetersTicket(string titleOverride = null)
            : base(titleOverride)
        {
            if (string.IsNullOrEmpty(titleOverride))
            {
                UpdateTitle(TicketLocalizer.GetString(ResourceKeys.MainAccountingMeters));
            }
        }

        public override void AddTicketContent()
        {
            var configMeters = ConfigurationUtilities.GetConfiguration(
                MetersExtensionPath,
                () => new DisplayMetersConfiguration { MeterNodes = new MeterNode[0] });
            var pageMeters = configMeters.MeterNodes.Where(m => m.Page == MeterNodePage.MainPage);
            foreach (var node in pageMeters)
            {
                MeterNodes.Add(node);
            }

            MeterNodes = MeterNodes.OrderBy(n => n.Order).ToList();

            var meterManager = ServiceManager.GetService<IMeterManager>();
            foreach (var meterNode in MeterNodes)
            {
                if (meterManager.IsMeterProvided(meterNode.Name))
                {
                    var meter = meterManager.GetMeter(meterNode.Name);
                    AddLabeledLine(meterNode.DisplayName, meter.Classification.CreateValueString(meter.Lifetime), false);
                }
            }
        }
    }
}