namespace Aristocrat.Monaco.Sas.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts;
    using log4net;
    using Protocol.Common.Storage.Entity;
    using Storage.Models;

    /// <summary>
    ///     This class handles queuing of SAS exceptions to be sent to the host when
    ///     a general poll occurs.
    ///     It prioritizes the exceptions in the queue based on the SAS spec recommendations in
    ///     section "2.2.1 General Polls".
    ///     The queue is persisted.
    /// </summary>
    public class SasPriorityExceptionQueue : ISasExceptionQueue, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IEnumerable<GeneralExceptionCode> PersistedPriorityExceptions = new List<GeneralExceptionCode>
        {
            GeneralExceptionCode.HandPayWasReset, GeneralExceptionCode.HandPayIsPending
        };

        private const int QueueSize = 25;
        private readonly ISasExceptionCollection _noExceptionsPending = new GenericExceptionBuilder(GeneralExceptionCode.None);
        private Queue<ISasExceptionCollection> _exceptions;
        private readonly object _lockObject = new object();
        private bool _disposed;

        /// <summary>
        ///     The priority exceptions in the order given in the SAS Protocol Spec section 2.2.1 on page 2-2.
        ///     For secure handpay reporting we will flip the handpay reset and pending as we need to post the
        ///     handpay pending after the previous handpay was reset and the exception was posted.
        ///     Since the reset cannot be posted until after handpay pending is posted we always get the correct order.
        /// </summary>
        private readonly Dictionary<GeneralExceptionCode, int> _exceptionPriority =
            new Dictionary<GeneralExceptionCode, int>
            {
                { GeneralExceptionCode.SystemValidationRequest, 1 },
                { GeneralExceptionCode.TicketHasBeenInserted, 2 },
                { GeneralExceptionCode.TicketTransferComplete, 3 },
                { GeneralExceptionCode.ValidationIdNotConfigured, 4 },
                { GeneralExceptionCode.AftRequestForHostCashOut, 5 },
                { GeneralExceptionCode.AftRequestForHostToCashOutWin, 6 },
                { GeneralExceptionCode.GameLocked, 7 },
                { GeneralExceptionCode.SasProgressiveLevelHit, 8 },
                { GeneralExceptionCode.CashOutTicketPrinted, 9 },
                { GeneralExceptionCode.HandPayValidated, 10 },
                { GeneralExceptionCode.AftTransferComplete, 11 },
                { GeneralExceptionCode.AftRequestToRegister, 12 },
                { GeneralExceptionCode.AftRegistrationAcknowledged, 13 },
                { GeneralExceptionCode.HandPayWasReset, 14 },
                { GeneralExceptionCode.HandPayIsPending, 15 },
                { GeneralExceptionCode.AuthenticationComplete, 16 },
                { GeneralExceptionCode.ExceptionBufferOverflow, 17 }
            };
        
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IList<SasGroup> _registeredGroups = new List<SasGroup>();

        /// <summary>
        ///     flags that indicate which of the priority exceptions are currently active
        /// </summary>
        private int _priorityExceptionFlag;

        private readonly Dictionary<GeneralExceptionCode, List<Action>> _handlers = new Dictionary<GeneralExceptionCode, List<Action>>();
        private bool _pendingExceptionRead;
        private int _pendingPriorityException = -1;

        private readonly bool _discardOldestException;

        /// <summary>
        ///     Instantiates a new instance of the SasPriorityExceptionQueue class
        /// </summary>
        /// <param name="client">which client this queue is intended for, 1 or 2. Used to determine which persistent block to load/create</param>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="exceptionHandler"></param>
        /// <param name="configuration"></param>
        public SasPriorityExceptionQueue(byte client, IUnitOfWorkFactory unitOfWorkFactory, ISasExceptionHandler exceptionHandler, SasClientConfiguration configuration)
        {
            if (configuration.LegacyHandpayReporting)
            {
                _exceptionPriority[GeneralExceptionCode.HandPayIsPending] = 14;
                _exceptionPriority[GeneralExceptionCode.HandPayWasReset] = 15;
            }

            // for NONE validation type exceptions 3D/3E are normal exceptions
            if (configuration.IsNoneValidation)
            {
                _exceptionPriority.Remove(GeneralExceptionCode.CashOutTicketPrinted);
                _exceptionPriority.Remove(GeneralExceptionCode.HandPayValidated);
            }

            // load the persistent storage block for this component.
            ClientNumber = client;
            _unitOfWorkFactory = unitOfWorkFactory;
            _exceptionHandler = exceptionHandler;

            var exceptionQueue = _unitOfWorkFactory.Invoke(
                x => x.Repository<ExceptionQueue>().Queryable().FirstOrDefault(e => e.ClientId == ClientNumber));
            var persistedCollection = exceptionQueue != null
                ? StorageUtilities.GetListFromByteArray<ISasExceptionCollection>(exceptionQueue.Queue)
                : new List<ISasExceptionCollection>();
            _exceptions = new Queue<ISasExceptionCollection>(persistedCollection);

            var priorityExceptionMask = PersistedPriorityExceptions.Aggregate(0, (mask, code) => mask | (1 << _exceptionPriority[code]));
            _priorityExceptionFlag = priorityExceptionMask & (exceptionQueue?.ClientId ?? 0);

            RegisterExceptionHandling(SasGroup.PerClientLoad, true);
            RegisterExceptionHandling(SasGroup.Aft, configuration.HandlesAft);
            RegisterExceptionHandling(SasGroup.Validation, configuration.HandlesValidation);
            RegisterExceptionHandling(SasGroup.GeneralControl, configuration.HandlesGeneralControl);
            RegisterExceptionHandling(SasGroup.LegacyBonus, configuration.HandlesLegacyBonusing);
            RegisterExceptionHandling(SasGroup.Progressives, configuration.HandlesProgressives);
            RegisterExceptionHandling(SasGroup.GameStartEnd, configuration.HandlesGameStartEnd);
            _discardOldestException = configuration.DiscardOldestException;
        }

        /// <summary>
        ///     Gets a value indicating whether the queue is full or not
        /// </summary>
        public bool ExceptionQueueIsFull => _exceptions.Count == QueueSize;

        /// <inheritdoc/>
        public byte ClientNumber { get; }

        /// <inheritdoc/>
        public void AddHandler(GeneralExceptionCode exception, Action action)
        {
            if (_handlers.ContainsKey(exception))
            {
                _handlers[exception].Add(action);
            }
            else
            {
                _handlers.Add(exception, new List<Action> { action });
            }
        }

        /// <inheritdoc/>
        public void RemoveHandler(GeneralExceptionCode exception)
        {
            _handlers.Remove(exception);
        }

        /// <inheritdoc/>
        public void QueueException(ISasExceptionCollection exception)
        {
            lock (_lockObject)
            {
                var exceptionCode = ConvertRealTimeExceptionToNormal(exception);
                if (_exceptionPriority.ContainsKey(exceptionCode))
                {
                    QueuePriorityException(exceptionCode);
                    return;
                }

                var addCurrentException = true;
                // if the queue is full, drop the oldest event in the queue (unless otherwise specified by the jurisdiction)
                if (ExceptionQueueIsFull)
                {
                    if (_discardOldestException)
                    {
                        _exceptions.Dequeue(); //dropped oldest exception, so add current one.
                    }
                    else
                    {
                        addCurrentException = false; //keeping oldest exception, so drop current one
                    }
                    QueuePriorityException(GeneralExceptionCode.ExceptionBufferOverflow);
                }

                if (addCurrentException)
                {
                    _exceptions.Enqueue(exception);
                }

                // persist the queue
                Persist();
                Logger.Debug($"SAS event queue now contains {_exceptions.Count} exceptions");
            }
        }

        /// <inheritdoc/>
        public void QueuePriorityException(GeneralExceptionCode exception)
        {
            // set a flag based on the priority of the given exception
            _priorityExceptionFlag |= 1 << _exceptionPriority[exception];

            // persist the flag
            Persist();
            Logger.Debug($"Queued Priority exception :{_priorityExceptionFlag:X4}");
        }

        /// <inheritdoc/>
        public ISasExceptionCollection Peek()
        {
            lock (_lockObject)
            {
                // Check first for a priority exception
                var (priorityExcCode, priorityExcVal) = GetPriorityException();
                if (priorityExcVal != 0)
                {
                    return new GenericExceptionBuilder(priorityExcCode);
                }

                // If no priority exceptions, check for normal exceptions
                var exception = _exceptions.Count > 0
                    ? _exceptions.Peek()
                    : _noExceptionsPending;
                return exception;
            }
        }

        /// <inheritdoc/>
        public ISasExceptionCollection GetNextException()
        {
            lock (_lockObject)
            {
                // Clear the pending exceptions as we are reading another and we don't want to clear the wrong exception
                ClearPendingException();

                var priorityException = GetNextPriorityException();
                if (priorityException != 0)
                {
                    return new GenericExceptionBuilder(priorityException);
                }

                var exception = _exceptions.Count > 0
                    ? _exceptions.Peek()
                    : _noExceptionsPending;
                Logger.Debug("GetNextException Response of:");
                foreach (var ex in exception)
                {
                    Logger.Debug($" {ex:X2}");
                }

                _pendingExceptionRead = !Equals(exception, _noExceptionsPending);
                return exception;
            }
        }

        /// <inheritdoc />
        public GeneralExceptionCode ConvertRealTimeExceptionToNormal(ISasExceptionCollection exception)
        {
            return exception.ExceptionCode;
        }

        /// <inheritdoc />
        public void ExceptionAcknowledged()
        {
            lock (_lockObject)
            {
                if (!_pendingExceptionRead)
                {
                    return;
                }

                if (_pendingPriorityException > 0)
                {
                    var (priorityException, _) = GetPriorityException();
                    InvokeHandler(priorityException);

                    _priorityExceptionFlag &= ~(1 << _pendingPriorityException);
                }
                else
                {
                    InvokeHandler(ConvertRealTimeExceptionToNormal(_exceptions.Dequeue()));
                }

                Persist();
                _pendingPriorityException = -1;
                _pendingExceptionRead = false;
            }
        }

        /// <inheritdoc />
        public void ClearPendingException()
        {
            lock (_lockObject)
            {
                _pendingPriorityException = -1;
                _pendingExceptionRead = false;
            }
        }

        /// <inheritdoc />
        public void RemoveException(ISasExceptionCollection exception)
        {
            lock (_lockObject)
            {
                var exceptionCode = ConvertRealTimeExceptionToNormal(exception);
                RemoveHandler(exceptionCode);
                if (_exceptionPriority.TryGetValue(exceptionCode, out var flag))
                {
                    _priorityExceptionFlag &= ~(1 << flag);
                    Persist();
                    return;
                }

                _exceptions = new Queue<ISasExceptionCollection>(_exceptions.Where(x => !x.SequenceEqual(exception)));
                Persist();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var group in _registeredGroups)
                {
                    _exceptionHandler.RemoveExceptionQueue(group, this);
                }

                _registeredGroups.Clear();
            }

            _disposed = true;
        }

        /// <summary>
        ///     Returns the current priority exception, zero if there isn't one
        /// </summary>
        private (GeneralExceptionCode, byte) GetPriorityException()
        {
            lock (_lockObject)
            {
                var priorityExcKey = GeneralExceptionCode.None;
                byte priorityExcVal = 0;
                if (_priorityExceptionFlag != 0)
                {
                    // go thru the flagged priority exceptions and get the highest priority one.
                    foreach (var kvp in _exceptionPriority.OrderBy(x => x.Value))
                    {
                        if ((_priorityExceptionFlag & 1 << kvp.Value) != 0)
                        {
                            Logger.Debug($"Found Priority exception :{(byte)kvp.Key:X2}");
                            priorityExcKey = kvp.Key;
                            priorityExcVal = (byte)kvp.Value;
                            break;
                        }
                    }
                }

                return (priorityExcKey, priorityExcVal);
            }
        }

        private GeneralExceptionCode GetNextPriorityException()
        {
            // if we don't have any priority exceptions just return 0
            if (_priorityExceptionFlag == 0)
            {
                return 0;
            }

            Logger.Debug($"Getting Priority exception :{_priorityExceptionFlag:X4}");

            (var priorityExcKey, byte priorityExcVal) = GetPriorityException();
            if (priorityExcVal != 0)
            {
                _pendingPriorityException = priorityExcVal;
                _pendingExceptionRead = true;
            }

            return priorityExcKey;
        }

        private void Persist()
        {
            Task.Run(
                () =>
                {
                    byte[] queue;
                    int priority;
                    lock (_lockObject)
                    {
                        queue = StorageUtilities.ToByteArray(_exceptions.ToList());
                        priority = _pendingPriorityException;
                    }

                    using var work = _unitOfWorkFactory.Create();
                    work.BeginTransaction(IsolationLevel.Serializable);
                    var repository = work.Repository<ExceptionQueue>();
                    var persistence = repository.Queryable().FirstOrDefault(x => x.ClientId == ClientNumber) ??
                                      new ExceptionQueue { ClientId = ClientNumber };
                    persistence.Queue = queue;
                    persistence.PriorityQueue = priority;

                    repository.AddOrUpdate(persistence);
                    work.Commit();
                });
        }

        private void RegisterExceptionHandling(SasGroup group, bool handlesGroup)
        {
            if (handlesGroup)
            {
                _registeredGroups.Add(group);
                _exceptionHandler.RegisterExceptionProcessor(group, this);
            }
        }

        private void InvokeHandler(GeneralExceptionCode code)
        {
            if (_handlers.ContainsKey(code) && _handlers[code] != null)
            {
                var list = _handlers[code];
                if (list.Count > 0)
                {
                    // make a copy of the action since we
                    // will be deleting the original
                    var action = new Action(list[0]);
                    Task.Run(() => action.Invoke());
                    list.RemoveAt(0);
                }
            }
        }
    }
}