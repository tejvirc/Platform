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

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="eventBus"></param>
        /// <param name="properties"></param>
        public IdleTextService(IDispatcher dispatcher, IEventBus eventBus, IPropertiesManager properties)
        {
            _dispatcher = dispatcher;
            _eventBus = eventBus;
            _properties = properties;

            SubscribeToEvents();
        }

        /// <inheritdoc />
        public string? GetCabinetIdleText()
        {
            return _properties.GetValue<string?>(GamingConstants.IdleText, null);
        }

        /// <inheritdoc />
        public string GetDefaultIdleText()
        {
            return Localizer.For(CultureFor.Player).GetString(ResourceKeys.IdleTextDefault);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<PropertyChangedEvent>(this, Handle);
        }

        private void Handle(PropertyChangedEvent evt)
        {
            if (evt.PropertyName == GamingConstants.IdleText)
            {
                UpdateCabinetIdleText();
            }
        }

        private void UpdateCabinetIdleText()
        {
            var text = _properties.GetValue<string?>(GamingConstants.IdleText, null);
            _dispatcher.Dispatch(new BannerUpdateIdleTextAction(IdleTextType.CabinetOrHost, text));
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
