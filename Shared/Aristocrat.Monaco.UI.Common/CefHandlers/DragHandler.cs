namespace Aristocrat.Monaco.UI.Common.CefHandlers
{
    using System;
    using System.Collections.Generic;
    using CefSharp;
    using CefSharp.Enums;

    /// <inheritdoc />
    public class DragHandler : IDragHandler
    {
        /// <inheritdoc />
        public bool OnDragEnter(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IDragData dragData,
            DragOperationsMask mask)
        {
            // Do not allow dragging onto browser
            return true;
        }

        /// <inheritdoc />
        public void OnDraggableRegionsChanged(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IList<DraggableRegion> regions)
        {
        }
    }
}
