namespace Aristocrat.Monaco.Asp.Progressive
{
    using System;
    using System.Collections.Generic;

    public class OnNotificationEventArgs : EventArgs
    {
        public IReadOnlyDictionary<int, IReadOnlyList<string>> Notifications { get; }

        public OnNotificationEventArgs(IReadOnlyDictionary<int, IReadOnlyList<string>> notifications)
        {
            Notifications = notifications;
        }
    }
}