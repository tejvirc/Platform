namespace Aristocrat.Monaco.Sas.Exceptions
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     A Reel N Has Stopped exception builder
    /// </summary>
    [Serializable]
    public class ReelNHasStoppedExceptionBuilder : List<byte>, ISasExceptionCollection
    {
        private const int MinimumReelNumber = 1;
        private const int MaximumReelNumber = 9;
        private const byte MinimumStopValue = 0x00;
        private const byte MaximumStopValue = 0xFF;
        private const byte DefaultStopValue = 0xFF;

        /// <summary>
        ///     Creates a ReelNHasStoppedExceptionBuilder
        /// </summary>
        /// <param name="reelNumber">Reel number of the stopped reel.  Only the first 9 reels may be reported.</param>
        /// <param name="physicalStop">Physical stop.  Any stops above 255 must be reported as 255.</param>
        public ReelNHasStoppedExceptionBuilder(int reelNumber, int physicalStop)
        {
            if (reelNumber is < MinimumReelNumber or > MaximumReelNumber)
            {
                return;
            }

            if (physicalStop is < MinimumStopValue or > MaximumStopValue)
            {
                physicalStop = DefaultStopValue;
            }

            ExceptionCode = GeneralExceptionCode.ReelNHasStopped;

            Add((byte)ExceptionCode);
            Add((byte)reelNumber);
            Add((byte)physicalStop);
        }

        /// <inheritdoc />
        public GeneralExceptionCode ExceptionCode { get; }
    }
}