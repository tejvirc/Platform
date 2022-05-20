namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Data.Printers;
    using Handlers.Printer;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Monaco.Common.Storage;

    /// <summary>
    ///     An implementation of <see cref="IPrintLog" />
    /// </summary>
    public class PrintLogService : IPrintLog, IDisposable
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IIdProvider _idProvider;
        private readonly IPrintLogRepository _printLogs;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintLogService" /> class.
        /// </summary>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="printLogs">An <see cref="IPrintLogRepository" /> instance.</param>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="idProvider">An <see cref="IIdProvider" /> instance.</param>
        public PrintLogService(
            IMonacoContextFactory contextFactory,
            IPrintLogRepository printLogs,
            IEventBus eventBus,
            IG2SEgm egm,
            IEventLift eventLift,
            IIdProvider idProvider)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _printLogs = printLogs ?? throw new ArgumentNullException(nameof(printLogs));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));

            _eventBus.Subscribe<PrintRequestedEvent>(this, HandleRequested);
            _eventBus.Subscribe<PrintCompletedEvent>(this, HandleCompleted);
            //// VLT-2821: We technically don't support a failed print of a voucher.  If we start logging other print requests, we should add this back in.
            ////_eventBus.Subscribe<ErrorWhilePrintingEvent>(this, evt => HandleError((ErrorWhilePrintingEvent)evt));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public int MinimumLogEntries => Constants.DefaultMinLogEntries;

        /// <inheritdoc />
        public int Entries
        {
            get
            {
                using (var context = _contextFactory.Create())
                {
                    return _printLogs.Count(context);
                }
            }
        }

        /// <inheritdoc />
        public long LastSequence
        {
            get
            {
                using (var context = _contextFactory.Create())
                {
                    return _printLogs.GetAll(context).Max(x => (long?)x.Id) ?? 0;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<PrintLog> GetLogs()
        {
            using (var context = _contextFactory.Create())
            {
                return _printLogs.GetAll(context).ToList();
            }
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleRequested(PrintRequestedEvent theEvent)
        {
            using (var context = _contextFactory.Create())
            {
                var last = _printLogs.GetAll(context).OrderByDescending(l => l.Id).FirstOrDefault();

                // Weird G2S requirement, but if the last transaction was a failed transaction we need to overwrite it
                if (last != null && !last.Complete && last.State != 1)
                {
                    last.PrinterId = theEvent.PrinterId;
                    last.PrintDateTime = theEvent.Timestamp;
                    last.TemplateIndex = theEvent.TemplateId;
                    last.State = 1;
                    last.Complete = false;

                    _printLogs.Update(context, last);
                }
                else
                {
                    var log = new PrintLog
                    {
                        TransactionId = _idProvider.GetNextTransactionId(),
                        PrinterId = theEvent.PrinterId,
                        PrintDateTime = theEvent.Timestamp,
                        TemplateIndex = theEvent.TemplateId,
                        State = 1,
                        Complete = false
                    };
                    _printLogs.Add(context, log);
                }

                var count = _printLogs.Count(context);
                if (count > MinimumLogEntries)
                {
                    var overflow =
                        _printLogs.GetAll(context).OrderBy(l => l.Id).Take(count - MinimumLogEntries).ToList();

                    _printLogs.DeleteAll(context, overflow);
                }
            }
        }

        private void HandleCompleted(PrintCompletedEvent theEvent)
        {
            using (var context = _contextFactory.Create())
            {
                var log = _printLogs.GetAll(context).OrderByDescending(l => l.Id).FirstOrDefault();

                if (log != null && log.State == 1)
                {
                    log.Complete = true;

                    _printLogs.Update(context, log);

                    var printer = _egm.GetDevice<IPrinterDevice>(theEvent.PrinterId);
                    if (printer != null)
                    {
                        _eventLift.Report(
                            printer,
                            EventCode.G2S_PTE102,
                            log.TransactionId,
                            printer.TransactionList(log.ToPrintLog()));
                    }
                }
            }
        }
    }
}