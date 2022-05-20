namespace Aristocrat.Monaco.Bootstrap
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Util;

    /// <summary>
    ///     An appender which batches the log events and asynchronously forwards them to any configured appenders.
    /// </summary>
    /// <inheritdoc />
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification ="disposed of in OnClose method")]
    public sealed class AsyncBufferingForwardingAppender : BufferingForwardingAppender
    {
        private const int DefaultIdleTime = 500;

        private readonly Sequencer<LoggingEvent[]> _sequencer;
        private Timer _idleFlushTimer;

        private TimeSpan _idleTimeThreshold;
        private DateTime _lastFlushTime;

        /// <summary>
        ///     Creates an instance of the <see cref="T:Aristocrat.Monaco.Bootstrap.AsyncBufferingForwardingAppender" />
        /// </summary>
        public AsyncBufferingForwardingAppender()
        {
            _sequencer = new Sequencer<LoggingEvent[]>(Process);
            _sequencer.OnException += (sender, args)
                => LogLog.Error(GetType(), "An exception occurred while processing LogEvents.", args.Exception);
        }

        private bool IsIdle => DateTime.UtcNow - _lastFlushTime >= _idleTimeThreshold;

        /// <summary>
        ///     Gets or sets the idle-time in milliseconds at which any pending logging events are flushed.
        ///     <value>The idle-time in milliseconds.</value>
        ///     <remarks>
        ///         <para>
        ///             The value should be a positive integer representing the maximum idle-time of logging events
        ///             to be collected in the <see cref="AsyncBufferingForwardingAppender" />. When this value is
        ///             reached, buffered events are then flushed. By default the idle-time is <c>500</c> milliseconds.
        ///         </para>
        ///         <para>
        ///             If the <see cref="IdleTime" /> is set to a value less than or equal to <c>0</c>
        ///             then use the default value is used.
        ///         </para>
        ///     </remarks>
        /// </summary>
        public int IdleTime { get; set; } = DefaultIdleTime;

        /// <inheritdoc />
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (IdleTime <= 0)
            {
                IdleTime = DefaultIdleTime;
            }

            _idleTimeThreshold = TimeSpan.FromMilliseconds(IdleTime);
            _idleFlushTimer = new Timer(InvokeFlushIfIdle, null, _idleTimeThreshold, _idleTimeThreshold);
        }

        /// <inheritdoc />
        protected override void SendBuffer(LoggingEvent[] events)
        {
            if (!_sequencer.ShutdownRequested)
            {
                _sequencer.Enqueue(events);
            }
            else
            {
                base.SendBuffer(events);
            }
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            _idleFlushTimer.Dispose();
            _sequencer.Shutdown();

            Flush();

            base.OnClose();
        }

        private void Process(LoggingEvent[] logEvents)
        {
            base.SendBuffer(logEvents);
            _lastFlushTime = DateTime.UtcNow;
        }

        /// <summary>
        ///     This only flushes if <see cref="BufferingAppenderSkeleton.Lossy" /> is <c>False</c>.
        /// </summary>
        private void InvokeFlushIfIdle(object _)
        {
            if (!IsIdle)
            {
                return;
            }
            if (_sequencer.ShutdownRequested)
            {
                return;
            }

            Flush();
        }
    }
}
