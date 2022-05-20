namespace Aristocrat.Monaco.Application.UI.LampTest
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
#if !(RETAIL)
    using Events;
#endif
    using Contracts.LampTest;
    using Hardware.Contracts.ButtonDeck;
    using Kernel;

    [CLSCompliant(false)]
    public class LCDLampTest : ILampTest
    {
        private readonly IButtonDeckDisplay _buttonDeckDisplay;
        private SelectedLamps _selectedLamps;

        private static readonly ICollection<byte[]> BetButtons = new List<byte[]>
        {
            CreateImage(800, 256, Color.Black),
            CreateImage(800, 256, Color.White)
        };

        private static readonly ICollection<byte[]> BashButtons = new List<byte[]>
        {
            CreateImage(240, 320, Color.Black),
            CreateImage(240, 320, Color.White)
        };

        public LCDLampTest()
        : this(ServiceManager.GetInstance().GetService<IButtonDeckDisplay>())
        {
        }

        public LCDLampTest(IButtonDeckDisplay buttonDeckDisplay)
        {
            _buttonDeckDisplay = buttonDeckDisplay;
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
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
            var index = GetIndex(isOn);
            switch (_selectedLamps)
            {
                case SelectedLamps.All:
                    {
                        _buttonDeckDisplay?.Draw(0, BetButtons.ElementAt(index));
                        _buttonDeckDisplay?.Draw(1, BashButtons.ElementAt(index));
                        break;
                    }
                case SelectedLamps.Bash:
                    {
                        _buttonDeckDisplay?.Draw(1, BashButtons.ElementAt(index));
                        break;
                    }
            }
#if !(RETAIL)
            ServiceManager.GetInstance().GetService<IEventBus>().Publish(new LampTestLampStateEvent(_selectedLamps.ToString(), isOn));
#endif
        }

        public void SetSelectedLamps(SelectedLamps selectedLamps, bool turnOn)
        {
            _selectedLamps = selectedLamps;
            TurnOffAllLamps();
            if (turnOn)
            {
                SetLampState(true);
            }
        }

        private void Load()
        {
            TurnOffAllLamps();
            _selectedLamps = SelectedLamps.None;
        }

        private void Unload()
        {
            TurnOffAllLamps();
        }

        private void TurnOffAllLamps()
        {
            _buttonDeckDisplay?.Draw(0, BetButtons.ElementAt(0));
            _buttonDeckDisplay?.Draw(1, BashButtons.ElementAt(0));
        }

        private static int GetIndex(bool isOn)
        {
            return isOn ? 1 : 0;
        }

        private static byte[] CreateImage(int w, int h, Color color)
        {
            using (var image = new Aristocrat.Monaco.UI.Common.BitmapImage(w, h))
            {
                image.Canvas.Clear(color);
                return image.GetRawBytes();
            }
        }
    }
}
