namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;
    using System;
    using System.Windows.Controls;

    /// <summary>
    ///     Provides creation and management of Cef Browsers
    /// </summary>
    [CLSCompliant(false)]
    public interface IBrowserProcessManager : IService, IDisposable
    {
        /// <summary>
        ///     Initialize a new ChromiumWebBrowser and add it to the browser manager
        /// </summary>
        /// <param name="viewModel">IMediaPlayerViewModel that controls this browser</param>
        /// <returns>The new ChromiumWebBrowser instance</returns>
        ContentControl StartNewBrowser(IMediaPlayerViewModel viewModel);

        /// <summary>
        ///     Call this to release a browser from memory and dispose of it properly
        /// </summary>
        /// <param name="mediaPlayerView">Media Player View associated with the browser</param>
        void ReleaseBrowser(Control mediaPlayerView);
    }
}