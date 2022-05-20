namespace Aristocrat.Mgam.Client.Notification
{
    /// <summary>
    ///     The supported notification urgency levels.
    /// </summary>
    public enum NotificationUrgencyLevel
    {
        /// <summary>Issue to be addressed at earliest convenience, if needed</summary>
        Low = 0x00000001,

        /// <summary>Issue to be addressed within 24 hours</summary>
        Medium = 0x00000002,

        /// <summary>Issue to be addressed within 12 hours</summary>
        High = 0x00000003,

        /// <summary>Issue must be addressed immediately</summary>
        Critical = 0x00000004
    }
}
