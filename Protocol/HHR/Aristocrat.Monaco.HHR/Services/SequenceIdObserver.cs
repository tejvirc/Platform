namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using Client.WorkFlow;
    using Kernel;

    internal class SequenceIdObserver : IDisposable
    {
        private readonly IDisposable _sequenceIDisposable;

        public SequenceIdObserver(IPropertiesManager propertyManager, ISequenceIdManager sequenceIdManager)
        {
            _sequenceIDisposable = sequenceIdManager?.SequenceIdObservable.Subscribe(
                sequenceId => propertyManager.SetProperty(
                    HHRPropertyNames.SequenceId,
                    sequenceId),
                error => { }) ?? throw new ArgumentNullException(nameof(sequenceIdManager));
        }

        public void Dispose()
        {
            _sequenceIDisposable?.Dispose();
        }
    }
}