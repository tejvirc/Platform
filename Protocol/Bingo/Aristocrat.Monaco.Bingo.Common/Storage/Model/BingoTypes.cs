namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.ComponentModel;

    public enum BingoType
    {
        /// <summary>
        ///     An unknown BingoType
        ///     This strategy should never be used and only occur when we have not received the configuration from the server
        /// </summary>
        [Description("Unknown")]
        Unknown = 0,

        /// <summary>
        ///     The EGM is playing Custom Bingo
        /// </summary>
        [Description("Custom Bingo")]
        Custom,

        /// <summary>
        ///     The EGM is playing NIGC Bingo
        /// </summary>
        [Description("NIGC Bingo")]
        NIGC,

        /// <summary>
        ///     The EGM is playing Bonanza Bingo
        /// </summary>
        [Description("Bonanza Bingo")]
        Bonanza,

        /// <summary>
        ///     The EGM is playing Instant Bingo
        /// </summary>
        [Description("Instant Bingo")]
        Instant,

        /// <summary>
        ///     The EGM is playing Skill based bingo
        /// </summary>
        [Description("Skill")]
        Skill,

        /// <summary>
        ///     The EGM is playing Keno based bingo
        /// </summary>
        [Description("Keno")]
        Keno,

        /// <summary>
        ///     The EGM is playing Lottery based bingo
        /// </summary>
        [Description("Lottery")]
        Lottery,

        /// <summary>
        ///     The EGM is playing auto hold based bingo
        /// </summary>
        [Description("Auto Hold")]
        AutoHold,

        /// <summary>
        ///     The EGM is playing California Charity variant of Bingo
        /// </summary>
        [Description("California Charity Fixed Card")]
        CaliforniaCharity,

        /// <summary>
        ///     The maximum value for BingoType
        ///     This strategy should never be used
        /// </summary>
        MaxBingoType
    }
}