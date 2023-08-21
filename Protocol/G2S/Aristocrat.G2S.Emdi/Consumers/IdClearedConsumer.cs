namespace Aristocrat.G2S.Emdi.Consumers
{
    using Events;
    using Host;
    using log4net;
    using Monaco.Hardware.Contracts.IdReader;
    using Protocol.v21ext1b1;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Monaco.Kernel;

    /// <summary>
    ///     Consumes <see cref="IdClearedEvent"/> event
    /// </summary>
    public class IdClearedConsumer : Consumes<IdClearedEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IReporter _reporter;
        private readonly IIdReaderProvider _idReader;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdClearedConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public IdClearedConsumer(IReporter reporter)
        {
            _reporter = reporter;

            _idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>();
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(IdClearedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received IdClearedEvent event");
                
                await _reporter.ReportAsync(
                    new c_eventReportEventItem
                    {
                        eventCode = EventCodes.CardRemoved,
                        Item = await GetCardReaderStatus(theEvent.IdReaderId)
                    },
                    EventCodes.CardRemoved);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending card removed event report", ex);
            }
        }

        private async Task<object> GetCardReaderStatus(int id)
        {
            var adapter = _idReader?.Adapters.FirstOrDefault(x => x.IdReaderId == id);

            if (adapter == null)
            {
                throw new InvalidOperationException($"ID Reader {id} not found");
            }

            return await Task.FromResult(
                new cardStatus
                {
                    cardIn = false,
                    idNumber = string.Empty,
                    idReaderType = adapter.IdReaderType.ToString(),
                    idValidExpired =  false,
                    idReaderId = adapter.IdReaderId
                });
        }
    }
}
