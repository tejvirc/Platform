namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Global",
        Justification = "This gets set when created from server message and could be used by message handler in the future")]
    public class ClaimWinResults : IResponse
    {
        public ClaimWinResults(
            ResponseCode code,
            bool accepted,
            long gameSerial,
            uint cardSerial)
        {
            ResponseCode = code;
            Accepted = accepted;
            GameSerial = gameSerial;
            CardSerial = cardSerial;
        }

        public ResponseCode ResponseCode { get; }

        public bool Accepted { get; }

        public long GameSerial { get; }

        public uint CardSerial { get; }
    }
}
