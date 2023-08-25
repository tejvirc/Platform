namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;

    /// <summary>
    ///     Provides a read-only view into a jackpot transaction
    /// </summary>
    public interface IViewableJackpotTransaction
    {
        /// <summary>
        ///     Gets the transaction id
        /// </summary>
        long TransactionId { get; }

        /// <summary>
        ///     Gets the progressive Id
        /// </summary>
        int ProgressiveId { get; }

        /// <summary>
        ///     Gets the level identifier
        /// </summary>
        int LevelId { get; }

        /// <summary>
        ///     Gets the current state of the progressive
        /// </summary>
        ProgressiveState State { get; }

        /// <summary>
        ///     Gets the game identifier
        /// </summary>
        int GameId { get; }

        /// <summary>
        ///     Gets the denomination
        /// </summary>
        long DenomId { get; }

        /// <summary>
        ///     Gets the paytable win level index
        /// </summary>
        int WinLevelIndex { get; }

        /// <summary>
        ///     Gets the reset value.
        /// </summary>
        /// <value>The reset value.</value>
        long ResetValue { get; }

        /// <summary>
        ///     Gets the progressive value amount
        /// </summary>
        long ValueAmount { get; }

        /// <summary>
        ///     Gets the text representation of the progressive value; set by the EGM to the last known text value when
        ///     the progressive hit occurred
        /// </summary>
        string ValueText { get; }

        /// <summary>
        ///     Gets a strictly increasing series; set by the EGM to the last known sequence value when the progressive hit
        ///     occurred
        /// </summary>
        long ValueSequence { get; }

        /// <summary>
        ///     Gets the identifier of the bonus.
        /// </summary>
        /// <value>The identifier of the bonus.</value>
        string BonusId { get; }

        /// <summary>
        ///     Gets the value of the prize awarded to the EGM when the progressive hit was processed; due to delays
        ///     sending
        ///     updates to the EGM or simultaneous wins, this value may be different than the value reported by the EGM in the
        ///     progressiveHit command.
        /// </summary>
        long WinAmount { get; }

        /// <summary>
        ///     Gets the text representation of the progressive value; set by the host to the text value for
        ///     the prize awarded to the EGM when the progressive hit was processed
        /// </summary>
        string WinText { get; }

        /// <summary>
        ///     Gets a strictly increasing series; set by the host to the sequence value for the prize awarded to the EGM
        ///     when the progressive hit was processed; due to delays sending updates to the EGM or simultaneous wins, this value may be
        ///     different than the value reported by the EGM in the progressiveHit command.
        /// </summary>
        /// <value>The window sequence.</value>
        long WinSequence { get; }

        /// <summary>
        ///     Gets the method of payment for the award.
        /// </summary>
        /// <value>The pay method.</value>
        PayMethod PayMethod { get; }

        /// <summary>
        ///     Gets the value of the progressive payment; set by the EGM to the value of the prize actually paid to the
        ///     player; due to rounding, this value may be different than the value specified by the host
        /// </summary>
        long PaidAmount { get; }

        /// <summary>
        ///     Gets the progressive Exception Code
        /// </summary>
        byte Exception { get; }

        /// <summary>
        ///     Gets the Date/time that the progressive win was awarded
        /// </summary>
        DateTime PaidDateTime { get; }

        /// <summary>
        ///     The value of the hidden pool (in millicents) at the time of jackpot
        /// </summary>
        long HiddenTotal { get; }

        /// <summary>
        ///     The value of the bulk pool (in millicents) at the time of jackpot
        /// </summary>
        long BulkTotal { get; }

        /// <summary>
        ///     The overflow amount at the time of jackpot
        /// </summary>
        long Overflow { get; }
    }
}