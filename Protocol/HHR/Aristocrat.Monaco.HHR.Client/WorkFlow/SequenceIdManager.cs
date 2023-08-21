namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System;
    using System.Reactive.Subjects;

    /// <summary>
    ///     Maintains sequence id required to send messages to Central Server.
    /// </summary>
    public class SequenceIdManager : ISequenceIdManager, IDisposable
    {
        private readonly Subject<uint> _sequenceIdSubject = new Subject<uint>();
        private uint _sequenceId;
        private bool _disposed;
        private readonly object _lock = new object();

        /// <summary>
        /// </summary>
        /// <param name="sequenceId"></param>
        public SequenceIdManager(uint sequenceId)
        {
            SequenceId = sequenceId;
        }

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private uint SequenceId
        {
            get => _sequenceId;
            set => _sequenceId = value == 0 ? 1 : value;
        }

        /// <inheritdoc />
        public uint NextSequenceId
        {
            get
            {
                lock (_lock)
                {
                    ++SequenceId;
                    _sequenceIdSubject.OnNext(SequenceId);
                    return SequenceId;
                }
            }
        }

        /// <inheritdoc />
        public IObservable<uint> SequenceIdObservable => _sequenceIdSubject;


        /// <summary>
        ///     Dispose
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
                _sequenceIdSubject.Dispose();
            }

            _disposed = true;
        }
    }
}