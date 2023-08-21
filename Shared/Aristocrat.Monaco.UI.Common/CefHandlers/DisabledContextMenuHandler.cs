namespace Aristocrat.Monaco.UI.Common.CefHandlers
{
    using System;
    using CefSharp;

    /// <inheritdoc cref="IContextMenuHandler" />
    public class DisabledContextMenuHandler : IContextMenuHandler
    {
        /// <inheritdoc />
        public void OnBeforeContextMenu(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            IMenuModel model)
        {
            model.Clear();
        }
        
        /// <inheritdoc />
        public bool OnContextMenuCommand(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            CefMenuCommand commandId,
            CefEventFlags eventFlags)
        {
            return true;
        }
        
        /// <inheritdoc />
        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
        }
        
        /// <inheritdoc />
        public bool RunContextMenu(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            IMenuModel model,
            IRunContextMenuCallback callback)
        {
            callback.Cancel();
            return true;
        }
    }
}
