namespace Aristocrat.Monaco.Gaming.UI.Views.Controls
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;
    using Contracts;
    using Kernel;
    using Monaco.UI.Common;
    using Runtime;
    using log4net;
    using Runtime.Client;

    public class NativeFormsControl : Control
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IRuntime _runtime;
        private DisplayId _displayId = 0;

        public NativeFormsControl()
        {
            _runtime = ServiceManager.GetInstance().GetService<IContainerService>().Container.GetInstance<IRuntime>();
            WindowsServices.RegisterTouchWindow(Handle, WindowsServices.TouchWindowFlag.FineTouch);
        }

        public void SetDisplayId(DisplayId displayId)
        {
            _displayId = displayId;
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg >= WindowsServices.WM_POINTERUPDATE && msg.Msg <= WindowsServices.WM_POINTERUP
                || msg.Msg >= WindowsServices.WM_MOUSEMOVE && msg.Msg <= WindowsServices.WM_LBUTTONUP)
            {
                var pointerX = msg.LParam.ToInt32() & 0xffff;
                var pointerY = msg.LParam.ToInt32() >> 16;
                var point = new Point(pointerX, pointerY);
                point = PointToClient(point);

                if (msg.Msg >= WindowsServices.WM_POINTERUPDATE && msg.Msg <= WindowsServices.WM_POINTERUP)
                {
                    var pointerId = (uint) (msg.WParam.ToInt32() & 0xffff);
                    var touchState = (TouchState) (msg.Msg - WindowsServices.WM_POINTERUPDATE + 1);
                    _runtime.SendTouch(_displayId, pointerId, touchState, (uint) point.X, (uint) point.Y);
                }
                else if (msg.Msg >= WindowsServices.WM_MOUSEMOVE && msg.Msg <= WindowsServices.WM_LBUTTONUP)
                {
                    var mouseButton = (MouseButton) (msg.Msg == WindowsServices.WM_MOUSEMOVE ? 0 : 1);
                    var mouseState = (MouseState) (msg.Msg - WindowsServices.WM_MOUSEMOVE + 1);
                    _runtime.SendMouse(_displayId, mouseButton, mouseState, (uint) point.X, (uint) point.Y);
                }
            }
            else
            {
                base.WndProc(ref msg);
            }
        }
    }
}