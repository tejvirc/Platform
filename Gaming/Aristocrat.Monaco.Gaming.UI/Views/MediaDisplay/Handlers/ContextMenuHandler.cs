namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using CefSharp;

    public class ContextMenuHandler : IContextMenuHandler
    {
        public void OnBeforeContextMenu(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            IContextMenuParams parameters,
            IMenuModel model)
        {
            model.Clear();
        }

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

        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {
            
        }

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
