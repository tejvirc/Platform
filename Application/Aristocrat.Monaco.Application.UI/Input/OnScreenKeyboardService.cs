namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Monaco.Application.Contracts.Input;
    using Contracts;
    using Kernel;
    using log4net;

    public class OnScreenKeyboardService : IOnScreenKeyboardService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IVirtualKeyboardProvider _keyboardProvider;

        private bool _disableKeyboard;
        private bool _disposed;

        public OnScreenKeyboardService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>())
        {
        }

        internal OnScreenKeyboardService(IEventBus eventBus, IPropertiesManager propertiesManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            var configuredKeyboardProvider = propertiesManager.GetValue(ApplicationConstants.KeyboardProvider, KeyboardProviderType.Default);
            _keyboardProvider = GetKeyboardProvider(configuredKeyboardProvider);
        }

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IOnScreenKeyboardService) };

        public bool DisableKeyboard
        {
            get => _disableKeyboard;
            set
            {
                if (_disableKeyboard == value)
                {
                    return;
                }

                _disableKeyboard = value;

                if (_disableKeyboard)
                {
                    CloseOnScreenKeyboard();
                }

                Logger.Debug($"DisableKeyboard: {_disableKeyboard}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            _eventBus.Subscribe<OnscreenKeyboardOpenedEvent>(this, HandleOnScreenKeyboardOpened);
            _eventBus.Subscribe<OnscreenKeyboardClosedEvent>(this, HandleOnScreenKeyboardClosed);
        }

        public void OpenOnScreenKeyboard(object targetControl)
        {
            if (DisableKeyboard)
            {
                Logger.Warn("OpenOnScreenKeyboard - Not opened due to disabled state, returning");
                return;
            }

            _keyboardProvider?.OpenKeyboard(targetControl);
        }

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

        private void CloseOnScreenKeyboard()
        {
            _keyboardProvider?.CloseKeyboard();
        }

        private void HandleOnScreenKeyboardClosed(OnscreenKeyboardClosedEvent e)
        {
            CloseOnScreenKeyboard();
        }

        private void HandleOnScreenKeyboardOpened(OnscreenKeyboardOpenedEvent e)
        {
            OpenOnScreenKeyboard(e.TargetControl);
        }

        private IVirtualKeyboardProvider GetKeyboardProvider(KeyboardProviderType configuredType)
        {
            return configuredType switch
            {
                KeyboardProviderType.Embedded => new EmbeddedKeyboardProvider(),
                KeyboardProviderType.Windows => new WindowsKeyboardProvider(),
                _ => null
            };
        }
    }
}
