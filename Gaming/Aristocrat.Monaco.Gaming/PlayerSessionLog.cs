namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Session;
    using Hardware.Contracts.IdReader;

    internal class PlayerSessionLog : IPlayerSessionLog
    {
        public long LogSequence { get; set; }
        public long TransactionId { get; set; }
        public IdReaderTypes IdReaderType { get; set; }
        public PlayerSessionState PlayerSessionState { get; set; }
        public string PlayerId { get; set; }
        public string IdNumber { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public long OverrideId { get; set; }
        public string ThemeId { get; set; }
        public string PaytableId { get; set; }
        public long DenomId { get; set; }
        public int HighestHotLevel { get; set; }
        public int CurrentHotLevel { get; set; }
        public long BasePointAward { get; set; }
        public long OverridePointAward { get; set; }
        public long PlayerPointAward { get; set; }
        public long HostPointAward { get; set; }
        public long HostCarryOver { get; set; }
        public long SessionCarryOver { get; set; }
        public long WageredCashableAmount { get; set; }
        public long WageredPromoAmount { get; set; }
        public long WageredNonCashAmount { get; set; }
        public long EgmPaidGameWonAmount { get; set; }
        public long HandPaidGameWonAmount { get; set; }
        public long EgmPaidProgWonAmount { get; set; }
        public long HandPaidProgWonAmount { get; set; }
        public long EgmPaidBonusWonAmount { get; set; }
        public long EgmPaidBonusNonWonAmount { get; set; }
        public long HandPaidBonusWonAmount { get; set; }
        public long HandPaidBonusNonWonAmount { get; set; }
        public long WonCount { get; set; }
        public long LostCount { get; set; }
        public long TiedCount { get; set; }
        public long TheoreticalPaybackAmount { get; set; }
        public long TheoreticalHoldAmount { get; set; }
        public int Exception { get; set; }
        public IEnumerable<SessionMeter> SessionMeters { get; set; } = Enumerable.Empty<SessionMeter>();

        public IPlayerSessionLog ShallowCopy()
        {
            return (IPlayerSessionLog)MemberwiseClone();
        }
    }
}
