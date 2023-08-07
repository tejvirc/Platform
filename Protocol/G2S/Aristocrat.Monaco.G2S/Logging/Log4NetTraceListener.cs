namespace Aristocrat.Monaco.G2S.Logging
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using log4net;

    /// <summary>
    ///     log4net TraceListener
    /// </summary>
    /// <seealso cref="System.Diagnostics.TraceListener" />
    internal class Log4NetTraceListener : TraceListener
    {
        private readonly ILog _logger;
        private TraceEventType _currentTraceEventType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Log4NetTraceListener" /> class.
        /// </summary>
        public Log4NetTraceListener()
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Log4NetTraceListener" /> class.
        /// </summary>
        /// <param name="logger">the logger to use</param>
        public Log4NetTraceListener(ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override void Write(string message)
        {
            WriteInternal(_currentTraceEventType, message);
        }

        /// <inheritdoc />
        public override void WriteLine(string message)
        {
            WriteInternal(_currentTraceEventType, message);
        }

        /// <inheritdoc />
        public override void TraceData(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            object data)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
            {
                return;
            }

            _currentTraceEventType = eventType;
            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <inheritdoc />
        public override void TraceData(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            params object[] data)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                return;
            }

            _currentTraceEventType = eventType;
            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <inheritdoc />
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null))
            {
                return;
            }

            _currentTraceEventType = eventType;
            base.TraceData(eventCache, source, eventType, id);
        }

        /// <inheritdoc />
        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string message)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
            {
                return;
            }

            _currentTraceEventType = eventType;
            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        /// <inheritdoc />
        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                return;
            }

            _currentTraceEventType = eventType;
            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        private void WriteInternal(TraceEventType eventType, string message)
        {
            if (_logger == null)
            {
                return;
            }

            switch (eventType)
            {
                case TraceEventType.Critical:
                    _logger.Fatal(message);
                    break;
                case TraceEventType.Error:
                    _logger.Error(message);
                    break;
                case TraceEventType.Warning:
                    _logger.Warn(message);
                    break;
                case TraceEventType.Information:
                    _logger.Info(message);
                    break;
                case TraceEventType.Verbose:
                    _logger.Debug(message);
                    break;
                default:
                    _logger.Info(message);
                    break;
            }
        }
    }
}