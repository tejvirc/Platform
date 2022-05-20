namespace Aristocrat.Bingo.Client.Messages
{
    public class Disable : IMessage
    {
        public Disable(string reason, bool forceCashout)
        {
            Reason = reason;
            ForceCashout = forceCashout;
        }

        public string Reason { get; }

        public bool ForceCashout { get; }
    }
}
