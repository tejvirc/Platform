namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Monaco.Localization.Properties;

    public class ButtonData
    {
        public ButtonData(int id, string resourceKey, bool enabled = false)
        {
            Id = id;
            ResourceKey = resourceKey;
            Enabled = enabled;
        }

        public int Id { get; }

        public string ResourceKey { get; }

        public bool Enabled { get; set; }
    }

    [CLSCompliant(false)]
    public class LCDButtonDeckViewModel : ObservableObject
    {
        private readonly ObservableCollection<ButtonData> _buttonData = new ObservableCollection<ButtonData>
        {
            new ButtonData(101, ResourceKeys.CashOut),
            new ButtonData(102, ResourceKeys.Button2Text),
            new ButtonData(103, ResourceKeys.Button3Text),
            new ButtonData(104, ResourceKeys.Button4Text),
            new ButtonData(105, ResourceKeys.Button5Text),
            new ButtonData(106, ResourceKeys.Button6Text),
            new ButtonData(107, ResourceKeys.Button7Text),
            new ButtonData(108, ResourceKeys.Button8Text),
            new ButtonData(109, ResourceKeys.Button9Text),
            new ButtonData(110, ResourceKeys.Button10Text),
            new ButtonData(111, ResourceKeys.Button11Text),
            new ButtonData(112, ResourceKeys.Button12Text),
            new ButtonData(113, ResourceKeys.BashButtonText)
        };

        public IList<ButtonData> ButtonData => _buttonData;

        public ButtonData Button(int id) => _buttonData.FirstOrDefault(b => b.Id == id);

        public int this[ButtonData data] => _buttonData.IndexOf(data);

        public void SetButtonEnabled(int id, bool enabled)
        {
            var button = Button(id);
            if (button is null || button.Enabled == enabled)
            {
                return;
            }

            _buttonData[this[button]] = new ButtonData(id, button.ResourceKey, enabled);
        }
    }
}
