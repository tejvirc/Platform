namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     Sas communication states
    /// </summary>
    public enum CommsStatus
    {
        /// <summary> Link between host and EGM is up </summary>
        Online,
        /// <summary> Link between host and EGM is down </summary>
        Offline,
        /// <summary> Loop break between EGM and Host </summary>
        LoopBreak
    }
}
