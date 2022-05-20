namespace Aristocrat.Monaco.Bingo
{
    public interface IBingoDisableProvider
    {
        /// <summary>
        ///     Disables the system
        /// </summary>
        /// <param name="reason">The reason for disabling</param>
        void Disable(string reason);

        /// <summary>
        ///     Enables the system
        /// </summary>
        void Enable();
    }
}
