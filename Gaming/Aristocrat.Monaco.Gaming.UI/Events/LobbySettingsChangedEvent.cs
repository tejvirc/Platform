using Aristocrat.Monaco.Kernel;

namespace Aristocrat.Monaco.Gaming.UI.Events
{
    public enum LobbySettingType
    {
        ServiceButtonVisible,
        ShowTopPickBanners
    }
    public class LobbySettingsChangedEvent : BaseEvent
    {
        public LobbySettingsChangedEvent(LobbySettingType settingType)
        {
            SettingType = settingType;
        }
        public LobbySettingType SettingType { get; }
    }
}
