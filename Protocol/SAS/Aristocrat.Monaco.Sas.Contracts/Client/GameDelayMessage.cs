namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using Kernel;

    /// <summary>
    ///     The message to display when the game is delayed through a 2E exception.
    /// </summary>
    public class GameDelayMessage
    {
        /// <summary>
        ///     The message to be displayed when a 2E exception occurs.
        /// </summary>
        public static readonly DisplayableMessage DisplayMessage = new DisplayableMessage(
            () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.GamePlaySuspended),
            DisplayableMessageClassification.Informative,
            DisplayableMessagePriority.Immediate,
            new Guid("{57F8929F-D843-4DBB-98D7-3DE335A5A33D}"));
    }
}
