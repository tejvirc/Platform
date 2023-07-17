namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using Monaco.Kernel;
    using MVVM.ViewModel;
    
    public class MaxWinDialogViewModel : BaseViewModel, IDisposable
    {
        private readonly IEventBus _eventBus;
        private string _maxWinAmount;
        private bool _disposed = false;

        /// <summary>
        ///     Maximum win amount value in $
        /// </summary>
        public string MaxWinAmount
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
        ///     Initializes a new instance of the <see cref="MaxWinDialogViewModel" /> class.
        /// </summary>
        public MaxWinDialogViewModel() : this(ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MaxWinDialogViewModel" /> class.
        /// </summary>
        /// <param name="eventBus">Event bus</param>
        public MaxWinDialogViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <summary>
        ///     Dispose of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Will unsubscribe from all events when disposing
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
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
