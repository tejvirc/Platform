namespace Aristocrat.G2S.Emdi.Host
{
    using System.Threading.Tasks;
    using Monaco.Application.Contracts.Media;

    /// <summary>
    /// This interface is used to extend the <see cref="IMediaProvider"/>
    /// </summary>
    public interface IMediaAdapter
    {
        /// <summary>
        /// Gets the media provider
        /// </summary>
        IMediaProvider Provider { get; }

        /// <summary>
        /// This method is used to encapsulate the logic for requesting <see cref="IMediaProvider"/>
        /// to show/hide media display window
        /// </summary>
        /// <param name="port"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<bool> SetDeviceVisibleAsync(int port, bool status);
    }
}