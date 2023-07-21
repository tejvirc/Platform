namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Contracts.Lobby;
    using Contracts.Models;
    using Kernel;

    /// <summary>
    ///     View Model for InfoWindow
    /// </summary>
    public class InfoOverlayViewModel : BaseObservableObject, IDisposable
    {
        /// <summary>
        ///     guid for the cabinet configuration text that we handle directly
        /// </summary>
        private readonly Guid _cabinetConfiguration = new Guid("E359897C-CE05-4E98-B4D9-F522EC07FB42");

        private bool _disposed;

        /// <summary>
        ///     Collection of text by guid for the top left area.
        /// </summary>
        private readonly Dictionary<Guid, string> _topLeftText = new Dictionary<Guid, string>();

        /// <summary>
        ///     Collection of text by guid for the top left area.
        /// </summary>
        private readonly Dictionary<Guid, string> _topRightText = new Dictionary<Guid, string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoOverlayViewModel" /> class
        /// </summary>
        public InfoOverlayViewModel()
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            eventBus.Subscribe<InfoOverlayTextEvent>(this, HandleEvent);

            var configName = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(ApplicationConstants.ActiveProtocol, string.Empty);

            _topRightText[_cabinetConfiguration] = configName;
        }

        /// <summary>
        ///     Gets text for the top left area
        /// </summary>
        /// <returns>list of text for the top left</returns>
        public string TextsTopLeft => string.Join(Environment.NewLine, _topLeftText.Values);

        /// <summary>
        ///     Gets text for the top right area
        /// </summary>
        /// <returns>list of text for the top right</returns>
        public string TextsTopRight => string.Join(Environment.NewLine, _topRightText.Values);

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
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleEvent(InfoOverlayTextEvent data)
        {
            switch (data.Location)
            {
                case InfoLocation.TopLeft:
                {
                    if (!data.Clear)
                    {
                        _topLeftText[data.TextGuid] = data.Text;
                    }
                    else
                    {
                        if (_topLeftText.ContainsKey(data.TextGuid))
                        {
                            _topLeftText.Remove(data.TextGuid);
                        }
                    }

                    break;
                }

                case InfoLocation.TopRight:
                {
                    if (!data.Clear)
                    {
                        _topRightText[data.TextGuid] = data.Text;
                    }
                    else
                    {
                        if (_topRightText.ContainsKey(data.TextGuid))
                        {
                            _topRightText.Remove(data.TextGuid);
                        }
                    }

                    break;
                }

                default:
                    break;
            }

            OnPropertyChanged(nameof(TextsTopLeft));
        }
    }
}
