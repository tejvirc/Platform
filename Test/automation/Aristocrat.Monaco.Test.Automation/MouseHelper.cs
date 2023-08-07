namespace Aristocrat.Monaco.Test.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using Point = System.Drawing.Point;

    public class MouseHelper
    {
        private IntPtr _buttonDeckWindow = IntPtr.Zero;

        private IntPtr _gameWindow = IntPtr.Zero;

        public List<string> TimeLimitButtons { get; set; } = new List<string>() { "btnOk", "btnYes" };

        public Action<string> Logger = null;

        /// <summary>
        ///     Send a mouse click to the game screen
        /// </summary>
        public void ClickGame(int x, int y)
        {
            try
            {
                _gameWindow = WindowHelper.FindScreen("Shell", Constants.MainWindName);

                Click(_gameWindow, x, y);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Logger?.Invoke(ex.ToString());
            }
            catch
            {
                Logger?.Invoke($"Unknown error while executing {System.Reflection.MethodBase.GetCurrentMethod()!.Name} Coordinate: {x},{y}");
            }
        }

        public void ClickOperatorMenu(int x, int y)
        {
            try
            {
                _gameWindow = WindowHelper.FindScreen("", Constants.OperatorWindowName);

                Click(_gameWindow, x, y);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Logger?.Invoke(ex.ToString());
            }
            catch
            {
                Logger?.Invoke($"Unknown error while executing {System.Reflection.MethodBase.GetCurrentMethod()!.Name} Coordinate: {x},{y}");
            }
        }

        public void ClickVirtualButtonDeck(int x, int y)
        {
            try
            {
                _buttonDeckWindow = WindowHelper.FindScreen("VirtualButtonDeckView", Constants.ButtonDeckWindowName);
     
                Click(_buttonDeckWindow, x, y);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Logger?.Invoke(ex.ToString());
            }
            catch
            {
                Logger?.Invoke($"Unknown error while executing {System.Reflection.MethodBase.GetCurrentMethod()!.Name} Coordinate: {x},{y}");
            }
        }

        private void Click(IntPtr window, int x, int y)
        {
            try
            {
                if (window == IntPtr.Zero)
                {
                    return;
                }

                var p = new Point(x, y);
                NativeMethods.ScreenToClient(window, ref p);

                NativeMethods.PostMessage(window, NativeMethods.WM_LBUTTONDOWN, (IntPtr)0x1, (IntPtr)((p.Y << 0x10) | p.X));
                NativeMethods.PostMessage(window, NativeMethods.WM_LBUTTONUP, (IntPtr)0x1, (IntPtr)((p.Y << 0x10) | p.X));
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Logger?.Invoke(ex.ToString());
            }
            catch
            {
                Logger?.Invoke($"Unknown error while executing {System.Reflection.MethodBase.GetCurrentMethod()!.Name} Coordinate: {x},{y}");
            }
        }

        /// <summary>
        ///     Send a mouse click to the Responsible Gaming TimeDialogLimit dialog.
        /// </summary>
        public void ClickRG()
        {
            var hWnd = WindowHelper.GetWindow(Constants.RgWindowName);

            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            var window = (Window)HwndSource.FromHwnd(hWnd)?.RootVisual;

            window?.Dispatcher?.Invoke(
                () =>
                {
                    try
                    {
                        Button buttonToPress = null;
                        foreach (var buttonName in TimeLimitButtons)
                        {
                            buttonToPress = WindowHelper.FindChild<Button>(window, buttonName);
                            if (buttonToPress != null &&
                                buttonToPress.Visibility == Visibility.Visible &&
                                buttonToPress.IsVisible)
                            {
                                buttonToPress.Dispatcher?.Invoke(
                                    () =>
                                    {
                                        try
                                        {
                                            var peer = new ButtonAutomationPeer(buttonToPress);

                                            var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

                                            invokeProv?.Invoke();
                                        }
                                        catch (System.ComponentModel.Win32Exception ex)
                                        {
                                            Logger?.Invoke(ex.ToString());
                                        }
                                        catch
                                        {
                                            Logger?.Invoke(
                                                $"Unknown error while executing {System.Reflection.MethodBase.GetCurrentMethod()!.Name}");
                                        }
                                    });

                                break;
                            }
                        }
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        Logger?.Invoke(ex.ToString());
                    }
                    catch
                    {
                        Logger?.Invoke(
                            $"Unknown error while executing {System.Reflection.MethodBase.GetCurrentMethod()!.Name}");
                    }
                });
        }

        public void ClickAuditMenuExit()
        {
            var hWnd = WindowHelper.GetWindow(Constants.OperatorWindowName);

            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            var window = (Window)HwndSource.FromHwnd(hWnd)?.RootVisual;

            window?.Dispatcher?.Invoke(
                () =>
                {
                    try
                    {
                        var buttonExit = WindowHelper.FindChild<Button>(window, Constants.ButtonExit);
                        if (buttonExit != null &&
                            buttonExit.Visibility == Visibility.Visible &&
                            buttonExit.IsVisible)
                        {
                            buttonExit.Dispatcher?.Invoke(
                                () =>
                                {
                                    try
                                    {
                                        var peer = new ButtonAutomationPeer(buttonExit);

                                        var invokeProv =
                                            peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

                                        invokeProv?.Invoke();
                                    }
                                    catch (System.ComponentModel.Win32Exception ex)
                                    {
                                        Logger?.Invoke(ex.ToString());
                                    }
                                    catch
                                    {
                                        Logger?.Invoke(
                                            $"Unknown error while executing {System.Reflection.MethodBase.GetCurrentMethod()!.Name}");
                                    }
                                });
                        }
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        Logger?.Invoke(ex.ToString());
                    }
                    catch
                    {
                        Logger?.Invoke(
                            $"Unknown error while executing {System.Reflection.MethodBase.GetCurrentMethod()!.Name}");
                    }
                });
        }
    }
}
