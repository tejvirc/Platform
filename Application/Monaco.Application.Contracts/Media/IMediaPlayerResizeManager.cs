namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;

    /// <summary>
    ///     
    /// </summary>
    public interface IMediaPlayerResizeManager : IService
    {
        /// <summary>
        ///     Returns true if any resizing is currently occurring
        /// </summary>
        bool IsResizing { get; }

        /// <summary>
        ///     Returns true if the media player with the passed in ID is resizing
        /// </summary>
        /// <param name="id">Media Player ID to check</param>
        bool IdIsResizing(int id);
    }
}
