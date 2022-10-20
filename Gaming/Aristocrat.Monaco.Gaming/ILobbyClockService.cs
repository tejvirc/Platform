namespace Aristocrat.Monaco.Gaming
{
    using System;

    public interface ILobbyClockService
    {
        public bool FlashingEnabled { get; set; }

        public event EventHandler<bool> Notify;
    }
}