namespace Aristocrat.G2S.Emdi.Host
{
    using Events;
    using System.Collections.Generic;

    /// <summary>
    /// Returns a list of supported events
    /// </summary>
    internal static class SupportedEvents
    {
        /// <summary>
        /// Gets a ist of supported events
        /// </summary>
        /// <returns>List of supported events</returns>
        public static IList<(string Code, string Text)> Get()
        {
            return new List<(string Code, string Text)>
            {
                (EventCodes.CallAttendantButtonPressed, "Call Attendant/Change Button Pressed"),
                (EventCodes.CardInserted, "Card Inserted"),
                (EventCodes.CardRemoved, "Card Removed"),
                (EventCodes.CardAbandoned, "Card Abandoned"),
                (EventCodes.GameIdle, "Game Idle"),
                (EventCodes.G2SGameIdle, "Game Idle"),
                (EventCodes.PrimaryGameEscrow, "Primary Game Escrow"),
                (EventCodes.G2SPrimaryGameEscrow, "Primary Game Escrow"),
                (EventCodes.PrimaryGameStarted, "Primary Game Started"),
                (EventCodes.G2SPrimaryGameStarted, "Primary Game Started"),
                (EventCodes.PrimaryGameEnded, "Primary Game Ended"),
                (EventCodes.G2SPrimaryGameEnded, "Primary Game Ended"),
                (EventCodes.ProgressivePending, "Progressive Pending"),
                (EventCodes.G2SProgressivePending, "Progressive Pending"),
                (EventCodes.SecondaryGameChoice, "Secondary Game Choice"),
                (EventCodes.G2SSecondaryGameChoice, "Secondary Game Choice"),
                (EventCodes.SecondaryGameEscrow, "Secondary Game Escrow"),
                (EventCodes.G2SSecondaryGameEscrow, "Secondary Game Escrow"),
                (EventCodes.SecondaryGameStarted, "Secondary Game Started"),
                (EventCodes.G2SSecondaryGameStarted, "Secondary Game Started"),
                (EventCodes.SecondaryGameEnded, "Secondary Game Ended"),
                (EventCodes.G2SSecondaryGameEnded, "Secondary Game Ended"),
                (EventCodes.PayGameResults, "Pay Game Results"),
                (EventCodes.G2SPayGameResults, "Pay Game Results"),
                (EventCodes.GameEnded, "Game Ended"),
                (EventCodes.G2SGameEnded, "Game Ended"),
                (EventCodes.G2SDisplayInterfaceOpen, "EGM Local Media Display Interface Open"),
                (EventCodes.G2SDisplayInterfaceClosed, "EGM Local Media Display Interface Closed"),
                (EventCodes.G2SDisplayDeviceShown, "Media Display Device Shown"),
                (EventCodes.EgmStateChanged, "EGM State Changed"),
                (EventCodes.LocaleChanged, "Locale Changed"),
            };
        }
    }
}
