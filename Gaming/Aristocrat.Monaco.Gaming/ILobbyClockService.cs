namespace Aristocrat.Monaco.Gaming
{
    public interface ILobbyClockService
    {
        public bool FlashingEnabled { get; set; }
        //public delegate void ShowClockEventHandler(object sender, bool shouldShow);
        public event LobbyClockService.ShowClockEventHandler Notify;
    }
}