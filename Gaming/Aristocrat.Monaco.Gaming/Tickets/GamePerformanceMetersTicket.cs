namespace Aristocrat.Monaco.Gaming.Tickets
{
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.MeterPage;
    using Application.Tickets;
    using Contracts;
    using Contracts.Meters;
    using Kernel;
    using Localization.Properties;

    public class GamePerformanceMetersTicket : AuditTicket
    {
        private const string MetersExtensionPath = "/Application/OperatorMenu/DisplayMeters";
        private readonly IGameDetail _game;
        protected List<MeterNode> MeterNodes = new List<MeterNode>();

        public GamePerformanceMetersTicket(IGameDetail game, string titleOverride = null)
            : base(titleOverride)
        {
            _game = game;

            if (string.IsNullOrEmpty(titleOverride))
            {
                UpdateTitle(TicketLocalizer.GetString(ResourceKeys.GamePerformanceMetersCaption));
            }
        }

        public override void AddTicketContent()
        {
            AddLabeledLine(ResourceKeys.GameName, _game.ThemeName);
            AddLabeledLine(ResourceKeys.GameId, _game.Version);
            AddLabeledLine(ResourceKeys.PaytableId, _game.PaytableId);

            var configMeters = ConfigurationUtilities.GetConfiguration(
                MetersExtensionPath,
                () => new DisplayMetersConfiguration { MeterNodes = new MeterNode[0] });
            var pageMeters = configMeters.MeterNodes.Where(m => m.Page == MeterNodePage.Game);
            foreach (var node in pageMeters)
            {
                MeterNodes.Add(node);
            }

            var meterManager = ServiceManager.GetService<IGameMeterManager>();
            foreach (var meterNode in MeterNodes.Where(x => meterManager.IsMeterProvided(x.Name)).OrderBy(n => n.Order))
            {
                var meter = meterManager.GetMeter(_game.Id, meterNode.Name);
                AddLabeledLine(
                    meterNode.DisplayName,
                    meter.Classification.CreateValueString(meter.Lifetime),
                    false);
            }
        }
    }
}