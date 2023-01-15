namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using Kernel.Contracts.MessageDisplay;
    using Kernel.MessageDisplay;

    /// <summary>
    ///     The message to display when the game is delayed through a 2E exception.
    /// </summary>
    public class GameDelayMessage
    {
        /// <summary>
        ///     The message to be displayed when a 2E exception occurs.
        /// </summary>
        public static readonly IDisplayableMessage DisplayMessage = new DisplayableMessage(
            ResourceKeys.GamePlaySuspended,
            CultureProviderType.Player,
            DisplayableMessageClassification.Informative,
            DisplayableMessagePriority.Immediate,
            null,
            new Guid("{57F8929F-D843-4DBB-98D7-3DE335A5A33D}"));
    }
}
