namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.ComponentModel;

    public enum CreditsStrategy
    {
        /// <summary>
        ///     Strategy to pay credits to the local machine
        /// </summary>
        [Description("Local")]
        Local = 0,

        /// <summary>
        ///     Strategy to pay credits by ticket in and out
        /// </summary>
        [Description("TITO Credits Strategy")]
        Tito,

        /// <summary>
        ///     Strategy to pay credits via the MGAM protocol
        /// </summary>
        [Description("MGAM")]
        Mgam,

        /// <summary>
        ///     Strategy to pay credits by PIN cash system
        /// </summary>
        [Description("PIN")]
        Pin,

        /// <summary>
        ///     Strategy to pay credits by SAS system
        /// </summary>
        [Description("SAS")]
        Sas,

        /// <summary>
        ///     Strategy to pay credits by Alesis system
        /// </summary>
        [Description("Alesis")]
        Alesis,

        /// <summary>
        ///     Strategy to pay credits by Caliente system
        /// </summary>
        [Description("Caliente")]
        Caliente,

        /// <summary>
        ///     An unknown credits strategy.
        ///     This should never be used and is invalid credits strategy
        /// </summary>
        [Description("Unknown")]
        Unknown = 255
    }
}