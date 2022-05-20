namespace Aristocrat.Monaco.Sas.ChangeRequests
{
    using System;
    using System.Collections.Generic;
    using Contracts.Client;
    using Kernel;

    /// <summary>Class that manages the Sas change requests</summary>
    public sealed class SasChangeRequestManager : ISasChangeRequestManager, IService, IDisposable
    {
        private readonly ISasMeterChangeHandler _meterChangeHandler;
        private readonly object _lock = new object();
        private readonly List<ISasChangeRequest> _requests = new List<ISasChangeRequest>();
        private bool _disposed;
        private const double CancelTimeout = 30_000; // 30 second cancellation timeout

        /// <summary>Constructs the SasChangeRequestManager object</summary>
        /// <param name="sasMeterChangeHandler">Handler for the Sas exceptions for pending, accepted, and cancelled meter changes</param>
        public SasChangeRequestManager(ISasMeterChangeHandler sasMeterChangeHandler)
        {
            _meterChangeHandler = sasMeterChangeHandler ?? throw new ArgumentNullException(nameof(sasMeterChangeHandler));
            _meterChangeHandler.OnChangeCommit += OnChangeCommit;
            _meterChangeHandler.OnChangeCancel += OnChangeCancel;
        }

        /// <inheritdoc />
        public string Name => typeof(SasChangeRequestManager).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(SasChangeRequestManager) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _meterChangeHandler.OnChangeCommit -= OnChangeCommit;
                _meterChangeHandler.OnChangeCancel -= OnChangeCancel;
            }

            _disposed = true;
        }

        /// <summary>
        ///     The method to add a change request for confirmation or denial from host before request is handled
        /// </summary>
        /// <param name="changeRequest">The change that is being requested</param>
        public void AddRequest(ISasChangeRequest changeRequest)
        {
            lock (_lock)
            {
                _requests.Add(changeRequest);
            }

            var meterCollectStatus = changeRequest.Type == ChangeType.Meters ?
                Aristocrat.Sas.Client.LongPollDataClasses.MeterCollectStatus.LifetimeMeterChange :
                Aristocrat.Sas.Client.LongPollDataClasses.MeterCollectStatus.GameDenomPaytableChange;
            _meterChangeHandler.StartPendingChange(meterCollectStatus, CancelTimeout);
        }

        private void OnChangeCommit(object sender, EventArgs e)
        {
            lock (_lock)
            {
                _requests.ForEach(item => item.Commit());
                _requests.Clear();
            }
        }

        private void OnChangeCancel(object sender, EventArgs e)
        {
            lock (_lock)
            {
                _requests.ForEach(item => item.Cancel());
                _requests.Clear();
            }
        }
    }
}
