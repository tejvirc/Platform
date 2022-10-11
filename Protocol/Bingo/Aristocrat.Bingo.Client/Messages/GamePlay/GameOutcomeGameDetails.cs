namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Global",
        Justification = "This gets set when created from server message and could be used by message handler in the future")]
    public class GameOutcomeGameDetails
    {
        public GameOutcomeGameDetails(
            int facadeKey,
            uint gameTitleId,
            uint themeId,
            int denominationId,
            string paytable,
            long gameSerial)
        {
            FacadeKey = facadeKey;
            GameTitleId = gameTitleId;
            ThemeId = themeId;
            DenominationId = denominationId;
            Paytable = paytable;
            GameSerial = gameSerial;
        }

        public int FacadeKey { get; }

        public uint GameTitleId { get; }

        public uint ThemeId { get; }

        public int DenominationId { get; }

        public string Paytable { get; }

        public long GameSerial { get; }
    }
}