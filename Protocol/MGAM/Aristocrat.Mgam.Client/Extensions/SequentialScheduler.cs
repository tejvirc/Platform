// ReSharper disable once CheckNamespace
namespace Aristocrat.Mgam.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A <see cref="TaskScheduler"/> that runs tasks sequentially on dedicated thread.
    /// </summary>
    public class SequentialScheduler : TaskScheduler, IDisposable
    {
        private readonly BlockingCollection<Task> _taskQueue = new BlockingCollection<Task>();
        private readonly Thread _thread;

        private volatile bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SequentialScheduler"/> class.
        /// </summary>
        public SequentialScheduler()
        {
            _thread = new Thread(Run);
            _thread.Start();
        }

        /// <inheritdoc />
        ~SequentialScheduler()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void  Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_taskQueue != null)
                {
                    _taskQueue.CompleteAdding();
                    Thread.Sleep(100);
                    _taskQueue.Dispose();
                }
            }

            _disposed = true;
        }

        /// <inheritdoc />
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _taskQueue;
        }

        /// <inheritdoc />
        protected override void QueueTask(Task task)
        {
            if (_taskQueue.IsAddingCompleted)
            {
                return;
            }

            _taskQueue.Add(task);
        }

        /// <inheritdoc />
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return !_disposed && Thread.CurrentThread == _thread && TryExecuteTask(task);
        }

        private void Run()
        {
            foreach (var task in _taskQueue.GetConsumingEnumerable())
            {
                if (!_disposed)
                {
                    TryExecuteTask(task);
                }
            }
        }
    }
}
