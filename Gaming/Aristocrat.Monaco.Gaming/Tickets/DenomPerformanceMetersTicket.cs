namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.MeterPage;
    using Application.Tickets;
    using Contracts.Meters;
    using Kernel;
    using Localization.Properties;

    public class DenomPerformanceMetersTicket : AuditTicket
    {
        private const string MetersExtensionPath = "/Application/OperatorMenu/DisplayMeters";
        private readonly long _denomMillicents;
        private readonly bool _isLifetime;
        protected List<MeterNode> MeterNodes = new List<MeterNode>();

        public DenomPerformanceMetersTicket(long denomMillicents, bool isLifetime = true, string titleOverride = null)
            : base(titleOverride)
        {
            _denomMillicents = denomMillicents;
            _isLifetime = isLifetime;

            if (string.IsNullOrEmpty(titleOverride))
            {
                UpdateTitle(TicketLocalizer.GetString(ResourceKeys.DenomPerformanceMeters));
            }
        }

        public override void AddTicketContent()
        {
            AddLabeledLine(ResourceKeys.Denomination, _denomMillicents.MillicentsToDollars().FormattedCurrencyString());

            var configMeters = ConfigurationUtilities.GetConfiguration(
                MetersExtensionPath,
                () => new DisplayMetersConfiguration { MeterNodes = new MeterNode[0] });
            var pageMeters = configMeters.MeterNodes.Where(m => m.Page == MeterNodePage.Denom);
            foreach (var node in pageMeters)
            {
                MeterNodes.Add(node);
            }

            MeterNodes = MeterNodes.OrderBy(n => n.Order).ToList();

            var meterManager = ServiceManager.GetService<IGameMeterManager>();
            foreach (var meterNode in MeterNodes)
            {
                if (meterManager.IsMeterProvided(meterNode.Name))
                {
                    var meter = meterManager.GetMeter(_denomMillicents, meterNode.Name);
                    AddLabeledLine(
                        meterNode.DisplayName,
                        meter.Classification.CreateValueString(_isLifetime ? meter.Lifetime : meter.Period),
                        false);
                }
            }
        }
    }
}