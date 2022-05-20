namespace Aristocrat.G2S.Emdi.Host
{
    /// <summary>
    /// The state of a connection between media display host and content
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// Session has not been validated or validation failed
        /// </summary>
        Invalid,

        /// <summary>
        /// Session has been validated
        /// </summary>
        Valid
    }
}