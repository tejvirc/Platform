namespace Aristocrat.Monaco.UI.Common.CefHandlers
{
    using System;
    using CefSharp;

    /// <inheritdoc />
    [CLSCompliant(false)]
    public class DisplayHandler : CefSharp.Handler.DisplayHandler
    {
        /// <inheritdoc />
        protected override bool OnConsoleMessage(
            IWebBrowser chromiumWebBrowser,
            ConsoleMessageEventArgs consoleMessageArgs)
        {
            // Don't log to the console
            return true;
        }

        /// <inheritdoc />
        protected override bool OnTooltipChanged(IWebBrowser chromiumWebBrowser, ref string text)
        {
            // Don't display tool tips
            return true;
        }
    }
}