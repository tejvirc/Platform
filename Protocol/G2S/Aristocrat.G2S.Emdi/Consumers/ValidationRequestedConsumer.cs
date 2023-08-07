namespace Aristocrat.G2S.Emdi.Consumers
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Events;
    using Host;
    using log4net;
    using Monaco.Hardware.Contracts.IdReader;
    using Monaco.Kernel;
    using Protocol.v21ext1b1;

    /// <summary>
    ///     Consumes <see cref="IdClearedEvent"/> event
    /// </summary>
    public class ValidationRequestedConsumer : Consumes<ValidationRequestedEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReporter _reporter;
        private readonly IIdReaderProvider _idReader;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationRequestedConsumer"/> class.
        /// </summary>
        /// <param name="reporter"></param>
        public ValidationRequestedConsumer(IReporter reporter)
        {
            _reporter = reporter;

            _idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>();
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(ValidationRequestedEvent theEvent)
        {
            try
            {
                await _reporter.ReportAsync(
                    new c_eventReportEventItem
                    {
                        eventCode = EventCodes.CardInserted,
                        Item = await GetCardReaderStatus(theEvent.IdReaderId, theEvent.TrackData?.IdNumber)
                    },
                    EventCodes.CardInserted);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending card inserted event report", ex);
            }
        }

        private async Task<object> GetCardReaderStatus(int id, string idNumber)
        {
            var adapter = _idReader?.Adapters.FirstOrDefault(x => x.IdReaderId == id);

            if (adapter == null)
            {
                throw new InvalidOperationException($"ID Reader {id} not found");
            }

            return await Task.FromResult(
                new cardStatus
                {
                    cardIn = true,
                    idNumber = idNumber,
                    idReaderType = adapter.IdReaderType.ToString(),
                    idValidExpired = false,
                    idReaderId = adapter.IdReaderId
                });
        }
    }
}
