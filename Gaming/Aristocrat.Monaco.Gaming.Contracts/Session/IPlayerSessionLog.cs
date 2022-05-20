namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Hardware.Contracts.IdReader;

    /// <summary>
    ///    Provides a mechanism to retrieve and interact with player session
    /// </summary>
    public interface IPlayerSessionLog : ILogSequence
    {
        /// <summary>
        ///  Unique transaction identifier assigned by the EGM
        /// </summary>
        long TransactionId { get; }

        /// <summary>
        ///    The type of the idReader device
        /// </summary>
        IdReaderTypes IdReaderType { get; }

        /// <summary>
        ///    Status of the player session entry
        /// </summary>
        PlayerSessionState PlayerSessionState { get; }

        /// <summary>
        ///  Player or other identifier assigned by the host system. 0-16 string
        /// </summary>
        string PlayerId { get; }

        /// <summary>
        ///  Identification number
        /// </summary>
        string IdNumber { get; }

        /// <summary>
        ///    Date and time that the session was started
        /// </summary>
        DateTime StartDateTime { get; }

        /// <summary>
        ///     Date and time that the session data was last updated
        /// </summary>
        DateTime EndDateTime { get; }

        /// <summary>
        ///    Last overrideId that was active during the session
        /// </summary>
        long OverrideId { get; }

        /// <summary>
        ///  Theme identifier of the last game played
        /// </summary>
        string ThemeId { get; }

        /// <summary>
        ///  Payable identifier of the last game played
        /// </summary>
        string PaytableId { get; }

        /// <summary>
        ///  Denom identifier of the last game played
        /// </summary>
        long DenomId { get; }

        /// <summary>
        ///   The highest hot player level achieved by the player.If set to 0 (zero) then the player never reached a hot player threshold.
        /// </summary>
        int HighestHotLevel { get; }

        /// <summary>
        ///   The last hot player level achieved by the player.If set to 0 (zero) then the player never reached a hot player threshold
        /// </summary>
        int CurrentHotLevel { get; }

        /// <summary>
        ///   Total session points that were awarded while using the base point parameters
        /// </summary>
        long BasePointAward { get; }

        /// <summary>
        ///    Total session points that were awarded while using the base point parameters
        /// </summary>
        long OverridePointAward { get; }

        /// <summary>
        ///   Total session points that were awarded while using the player-specific override parameters
        /// </summary>
        long PlayerPointAward { get; }

        /// <summary>
        ///   Total session points that were awarded by the host during the session
        /// </summary>
        long HostPointAward { get; }

        /// <summary>
        ///   Initial countdown carryover value that was set by the host
        /// </summary>
        long HostCarryOver { get; }

        /// <summary>
        ///   Final countdown carryover that was earned during the session
        /// </summary>
        long SessionCarryOver { get; }

        /// <summary>
        ///  Wagered cashable meter delta for the session.
        /// </summary>
        long WageredCashableAmount { get;  }

        /// <summary>
        ///    Wagered promo meter delta for the session
        /// </summary>
        long WageredPromoAmount { get;  }

        /// <summary>
        ///   Wagered non-cashable meter delta for the session
        /// </summary>
        long WageredNonCashAmount { get; }

        /// <summary>
        ///   EGM paid game won meter delta for the session
        /// </summary>
        long EgmPaidGameWonAmount { get;  }

        /// <summary>
        ///   EGM paid progressive won meter delta for the session
        /// </summary>
        long EgmPaidProgWonAmount { get; }

        /// <summary>
        ///   EGM paid bonus won meter delta for the session.
        /// </summary>
        long EgmPaidBonusWonAmount { get; }

        /// <summary>
        ///    EGM paid bonus non-won meter delta for the session
        /// </summary>
        long EgmPaidBonusNonWonAmount { get; }

        /// <summary>
        ///   Hand paid game won meter delta for the session
        /// </summary>
        long HandPaidGameWonAmount { get; }

        /// <summary>
        ///   Hand paid progressive won meter delta for the session
        /// </summary>
        long HandPaidProgWonAmount { get; }

        /// <summary>
        ///    Hand paid bonus won meter delta for the session
        /// </summary>
        long HandPaidBonusWonAmount { get; }

        /// <summary>
        ///    EGM paid bonus non-won meter delta for the session
        /// </summary>
        long HandPaidBonusNonWonAmount { get; }

        /// <summary>
        ///    Game won count meter delta for the session
        /// </summary>
        long WonCount { get; }

        /// <summary>
        ///    Game lost count meter delta for the session
        /// </summary>
        long LostCount { get; }

        /// <summary>
        ///    Game tied count meter delta for the session
        /// </summary>
        long TiedCount { get; }

        /// <summary>
        ///    Theoretical payback meter delta for the session
        /// </summary>
        long TheoreticalPaybackAmount { get; }

        /// <summary>
        ///   Theoretical hold meter delta for the session
        /// </summary>
        long TheoreticalHoldAmount { get; }

        /// <summary>
        ///     Gets the exception code
        /// </summary>
        int Exception { get; }

        /// <summary>
        ///     Gets the meter deltas for the session
        /// </summary>
        IEnumerable<SessionMeter> SessionMeters { get; }

        /// <summary>
        ///     Creates a shallow copy of the log
        /// </summary>
        /// <returns></returns>
        IPlayerSessionLog ShallowCopy();
    }
}
