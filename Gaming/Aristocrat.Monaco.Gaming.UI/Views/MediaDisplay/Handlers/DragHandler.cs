namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using System.Collections.Generic;
    using CefSharp;
    using CefSharp.Enums;

    public class DragHandler : IDragHandler
    {
        public bool OnDragEnter(IWebBrowser chromiumWebBrowser, IBrowser browser, IDragData dragData, DragOperationsMask mask)
        {
            // Do not allow dragging onto browser
            return true;
        }

        public void OnDraggableRegionsChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions)
        {
        }
    }
}
