namespace Aristocrat.Monaco.Gaming.UI.Views.Controls
{
    using System;
    using System.Reflection;
    using System.Windows.Forms;
    using Contracts;
    using Kernel;
    using Monaco.UI.Common;
    using Runtime;
    using log4net;

    public class NativeFormsControl : Control
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IRuntime _runtime;

        public NativeFormsControl()
        {
            _runtime = ServiceManager.GetInstance().GetService<IContainerService>().Container.GetInstance<IRuntime>();
            WindowsServices.RegisterTouchWindow(Handle, WindowsServices.TouchWindowFlag.FineTouch);
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WindowsServices.WM_TOUCH)
            {
                var inputCount = msg.WParam.ToInt32() & 0xffff;
                _runtime.SendTouch(inputCount, msg.LParam);
                msg.Result = IntPtr.Zero;
                return;
/*
                var inputs = new WindowsServices.TOUCHINPUT[inputCount];
                if (WindowsServices.GetTouchInputInfo(msg.LParam, inputCount, inputs))
                {
                    foreach (var input in inputs)
                    {
                        var xPos = input.x / 100;
                        var yPos = input.y / 100;
                        var fing = input.dwID;

                        if ((input.dwFlags & WindowsServices.TOUCHEVENTF_DOWN) > 0)
                        {
                            Logger.Debug($"GAME TOUCH DOWN {fing}@{xPos},{yPos}");
                        }
                        else if ((input.dwFlags & WindowsServices.TOUCHEVENTF_UP) > 0)
                        {
                            Logger.Debug($"GAME TOUCH UPPP {fing}@{xPos},{yPos}");
                        }
                        else if ((input.dwFlags & WindowsServices.TOUCHEVENTF_MOVE) > 0)
                        {
                            Logger.Debug($"GAME TOUCH MOVE {fing}@{xPos},{yPos}");
                        }
                    }

                    WindowsServices.CloseTouchInputHandle(msg.LParam);
                    msg.Result = IntPtr.Zero;
                    return;
                }*/
            }
            else if (msg.Msg >= WindowsServices.WM_LBUTTONDOWN && msg.Msg <= WindowsServices.WM_LBUTTONUP)
            {
                _runtime.SendMouse(msg.LParam);
                msg.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref msg);
        }
    }
}