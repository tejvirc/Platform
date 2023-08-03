namespace Aristocrat.Monaco.Application.UI.LampTest
{
    using System;
    using System.Collections.Generic;
#if !(RETAIL)
    using Events;
#endif
    using Contracts.LampTest;
    using Hardware.Contracts.IO;
    using Kernel;

    [CLSCompliant(false)]
    public class LampTest : ILampTest
    {
        private bool _enabled;
        private SelectedLamps _selectedLamps;
        private Dictionary<SelectedLamps, uint> _lampMap;
        private IIO _iio;

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (_enabled)
            {
                Load();
            }
            else
            {
                Unload();
            }
        }

        public void SetLampState(bool isOn)
        {
            _iio.SetButtonLampByMask(_lampMap[_selectedLamps], isOn);
#if !(RETAIL)
            ServiceManager.GetInstance().GetService<IEventBus>().Publish(new LampTestLampStateEvent(_selectedLamps.ToString(), isOn));
#endif
        }

        public void SetSelectedLamps(SelectedLamps selectedLamps, bool turnOn)
        {
            _selectedLamps = selectedLamps;
            _iio.SetButtonLampByMask(_lampMap[SelectedLamps.All], false);
            SetLampState(turnOn);
        }

        private void Initialize()
        {
            _enabled = false;
            _selectedLamps = SelectedLamps.None;
            _iio = ServiceManager.GetInstance().GetService<IIO>();

            _lampMap = new Dictionary<SelectedLamps, uint>
            {
                { SelectedLamps.All, 0xFFFF }, { SelectedLamps.Bash, 0x00C0 }, { SelectedLamps.None, 0x0000 }
            };
        }

        private void Load()
        {
            Initialize();
        }

        private void Unload()
        {
            SetLampState(false);
        }
    }
}