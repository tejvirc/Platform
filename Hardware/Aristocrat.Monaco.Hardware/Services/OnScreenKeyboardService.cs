namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using Contracts;
    using Contracts.Touch;
    using Kernel;
    using log4net;

    public class OnScreenKeyboardService : IOnScreenKeyboardService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;

        private Process _onScreenKeyboardProcess;
        private bool _disableKeyboard;

        public OnScreenKeyboardService()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        internal OnScreenKeyboardService(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
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

        public void Initialize()
        {
            _eventBus.Subscribe<OnscreenKeyboardOpenedEvent>(this, HandleOnScreenKeyboardOpened);
            _eventBus.Subscribe<OnscreenKeyboardClosedEvent>(this, HandleOnScreenKeyboardClosed);
        }

        public void OpenOnScreenKeyboard()
        {
            if (DisableKeyboard)
            {
                Logger.Warn("OnScreenKeyboard was not opened due to disabled state");
                return;
            }

            string path;
            try
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + @"\microsoft shared\ink\TabTip.exe";
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenOnScreenKeyboard Exception: {ex.Message} {ex.InnerException?.Message}");
                return;
            }

            try
            {
                var processStartInfo = new ProcessStartInfo(path);
                _onScreenKeyboardProcess = Process.Start(processStartInfo);
            }
            catch (Win32Exception ex)
            {
                Logger.Error($"OpenOnScreenKeyboard Win32Exception: [{ex.ErrorCode}] {ex.Message} {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenOnScreenKeyboard Exception: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        private void CloseOnScreenKeyboard()
        {
            if (_onScreenKeyboardProcess == null || _onScreenKeyboardProcess.HasExited)
            {
                return;
            }

            try
            {
                _onScreenKeyboardProcess.Kill();
            }
            catch (Win32Exception ex)
            {
                Logger.Error($"CloseOnScreenKeyboard Win32Exception: [{ex.ErrorCode}] {ex.Message} {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"CloseOnScreenKeyboard Exception: {ex.Message} {ex.InnerException?.Message}");
            }
            finally
            {
                _onScreenKeyboardProcess = null;
            }
        }

        private void HandleOnScreenKeyboardClosed(IEvent e)
        {
            var ignore = false;
            var onscreenKeyboardClosedEvent = (OnscreenKeyboardClosedEvent)e;
            if (onscreenKeyboardClosedEvent.IsTextBoxControl)
            {
                var serialTouchService = ServiceManager.GetInstance().TryGetService<ISerialTouchService>();
                ignore = serialTouchService != null ? !serialTouchService.IsManualTabletInputService : true;
            }

            if (!ignore)
            {
                CloseOnScreenKeyboard();
            }
        }

        private void HandleOnScreenKeyboardOpened(IEvent e)
        {
            var ignore = false;
            var onscreenKeyboardOpenedEvent = (OnscreenKeyboardOpenedEvent)e;
            if (onscreenKeyboardOpenedEvent.IsTextBoxControl)
            {
                var serialTouchService = ServiceManager.GetInstance().TryGetService<ISerialTouchService>();
                ignore = serialTouchService != null ? !serialTouchService.IsManualTabletInputService : true;
            }

            if (!ignore)
            {
                CloseOnScreenKeyboard();
                OpenOnScreenKeyboard();
            }
        }
    }
}
