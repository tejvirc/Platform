namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Aristocrat.Monaco.Gaming.UI.Events;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.UI.Common;
    using Aristocrat.MVVM.ViewModel;
    using System;
    using System.Linq;

    public class MaxWinDialogViewModel : BaseViewModel
    {
        private readonly IEventBus _eventBus;
        private long? _maxWinAmount;
        private bool _disposed = false;

        /// <summary>
        /// Maximum win amount value
        /// </summary>
        public long? MaxWinAmount
        {
            get
            {
                return _maxWinAmount;
            }
            set
            {
                _maxWinAmount = value;
                RaisePropertyChanged(nameof(MaxWinAmount));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxWinDialogViewModel" /> class.
        /// </summary>
        public MaxWinDialogViewModel() : this(ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxWinDialogViewModel" /> class.
        /// </summary>
        /// <param name="eventBus">Event bus</param>
        public MaxWinDialogViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <summary>
        ///  Dispose
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        /// <param name="disposing">True if disposing; false if finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
            }
            _disposed = true;
        }
    }
}
