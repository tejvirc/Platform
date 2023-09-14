namespace Aristocrat.Monaco.Gaming.Presentation.Services.IdleText
{
    using Application.Contracts.Localization;
    using Fluxor;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using Store;
    using System;

    public class IdleTextService : IIdleTextService, IDisposable
    {
        private bool _disposed;
        private readonly IDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

        public IdleTextService(IDispatcher dispatcher, IEventBus eventBus, IPropertiesManager properties)
        {
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _properties = properties;

            SubscribeToEvents();
        }

        public string GetDefaultIdleText()
        {
            // #TODO: Handle localized and jurisdiction-specific text from resource file...if not here then somewhere like view/viewmodel
            var defaultText = _properties.GetValue<string?>(GamingConstants.IdleText, null);
            if (string.IsNullOrEmpty(defaultText))
            {
                defaultText = Localizer.For(CultureFor.Player).GetString(ResourceKeys.IdleTextDefault);
            }

            return defaultText;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<PropertyChangedEvent>(this, Handle);
        }

        private void Handle(PropertyChangedEvent evt)
        {
            if (evt.PropertyName == GamingConstants.IdleText)
            {
                UpdateIdleText();
            }
        }

        private void UpdateIdleText()
        {
            var text = _properties.GetValue<string?>(GamingConstants.IdleText, null);
            _dispatcher.Dispatch(new BannerUpdateIdleTextAction(text));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

    }
}
