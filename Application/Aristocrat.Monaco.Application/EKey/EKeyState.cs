namespace Aristocrat.Monaco.Application.EKey
{
    /// <summary>
    ///     The EKey status of a smart card.
    /// </summary>
    public enum EKeyState
    {
        /// <summary>Initial state.</summary>
        Disconnected,

        /// <summary>Detected but not verified.</summary>
        Connected,

        /// <summary>Detected and verified.</summary>
        Verified
    }
}
