namespace Aristocrat.Monaco.Mgam.Services.DropMode
{
    /// <summary>
    ///     Defines the <see cref="IDropMode"/> interface.
    /// </summary>
    public interface IDropMode
    {
        /// <summary>
        ///     Get whether Drop Mode is active.
        /// </summary>
        bool Active { get; }

        /// <summary>
        ///     Clear the bill meters.
        /// </summary>
        void ClearMeters();
    }
}
