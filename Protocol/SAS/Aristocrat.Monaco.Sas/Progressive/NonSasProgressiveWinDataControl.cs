namespace Aristocrat.Monaco.Sas.Progressive
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Non Sas Progressive Win Data Control
    /// </summary>
    public class NonSasProgressiveWinDataControl
    {
        /// <summary>
        ///     Gets or sets Host 1 sent count
        /// </summary>
        public int Host1SentCount { get; set; }

        /// <summary>
        ///     Gets or sets Host 2 sent count
        /// </summary>
        public int Host2SentCount { get; set; }

        /// <summary>
        ///     Gets or sets Host 1 Wins
        /// </summary>
        public List<NonSasProgressiveWinData> Host1Wins { get; set; }

        /// <summary>
        ///     Gets or sets Host 2 Wins
        /// </summary>
        public List<NonSasProgressiveWinData> Host2Wins { get; set; }

        /// <summary>
        ///    Non Sas Progressive Win Data Control default constructor
        /// </summary>
        public NonSasProgressiveWinDataControl()
        {
            Host1Wins = new List<NonSasProgressiveWinData>();
            Host2Wins = new List<NonSasProgressiveWinData>();
        }
    }
}