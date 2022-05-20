namespace Aristocrat.Monaco.Test.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using System.Windows.Media;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using UI.Common;

    public class WindowHelper
    {
        /// <summary>
        ///     Delegate for the EnumChildWindows method
        /// </summary>
        /// <param name="hWnd">Window handle</param>
        /// <param name="parameter">Caller-defined variable; we use it for a pointer to our list</param>
        /// <returns>True to continue enumerating, false to bail.</returns>
        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        public static bool WindowExists(IntPtr hwnd)
        {
            return NativeMethods.IsWindow(hwnd);
        }

        /// <summary>
        ///     Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>
        ///     The first parent item that matches the submitted type parameter.
        ///     If not matching item can be found,
        ///     a null parent is being returned.
        /// </returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                if (!(child is T))
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null)
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    // If the child's name is set for search
                    if (!(child is FrameworkElement frameworkElement) || frameworkElement.Name != childName)
                    {
                        continue;
                    }

                    // if the child's name is of the request name
                    foundChild = (T)child;
                    break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        ///     Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>
        ///     The first parent item that matches the submitted type parameter.
        ///     If not matching item can be found,
        ///     a null parent is being returned.
        /// </returns>
        public static IOperatorMenuPageLoader FindChildPageLoader<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)
            {
                return null;
            }

            IOperatorMenuPageLoader foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                if (!(child is T))
                {
                    // recursively drill down the tree
                    foundChild = FindChildPageLoader<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null)
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    // If the child's name is set for search
                    if (child is ListBoxItem element &&
                        element.Content != null &&
                        element.Content is OperatorMenuPageLoader page &&
                        page.PageName == childName)
                    {
                        foundChild = page;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return foundChild;
        }

        public static IntPtr GetWindow(string title, string className = null)
        {
            return NativeMethods.FindWindow(className, title);
        }

        public static IntPtr GetWpfControl(string name, string windowName)
        {
            try
            {
                var hWnd = GetWindow(windowName);

                if (hWnd != IntPtr.Zero)
                {
                    var window = (Window)HwndSource.FromHwnd(hWnd)?.RootVisual;

                    if (window?.Dispatcher != null)
                    {
                        return window.Dispatcher.Invoke(
                            () =>
                            {
                                try
                                {
                                    var control = FindChild<Control>(window, name);

                                    if (control != null)
                                    {
                                        var source = (HwndSource)PresentationSource.FromVisual(control);

                                        if (source != null)
                                        {
                                            return source.Handle;
                                        }
                                    }
                                }
                                catch
                                {
                                }

                                return IntPtr.Zero;
                            });
                    }
                }
            }
            catch
            {
            }

            return IntPtr.Zero;
        }

        public static void GetAuditPage(string controlName, string windowName, string pageName)
        {
            try
            {
                var hWnd = GetWindow(windowName);

                if (hWnd != IntPtr.Zero)
                {
                    var window = (Window)HwndSource.FromHwnd(hWnd)?.RootVisual;

                    window?.Dispatcher?.Invoke(
                        () =>
                        {
                            try
                            {
                                var control = (TouchListBox)FindChild<Control>(window, controlName);

                                var item = FindChildPageLoader<ListBoxItem>(control, pageName);

                                control.Dispatcher?.Invoke(
                                    () => { control.SelectedItem = item; });
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        });
                }
            }
            catch
            {
            }
        }

        public static IntPtr GetWinFormControl(string name, string windowName)
        {
            var hWnd = GetWindow(windowName);
            try
            {
                if (hWnd != IntPtr.Zero)
                {
                    var wind = System.Windows.Forms.Control.FromHandle(hWnd);

                    if (wind != null)
                    {
                        foreach (System.Windows.Forms.Control ctrl in wind.Controls)
                        {
                            if (ctrl.Name == name)
                            {
                                return ctrl.Handle;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return IntPtr.Zero;
        }

        public static IntPtr FindScreen(string title, string name, string windowType = null)
        {
            try
            {
                var targetClass = windowType ?? "SimpleChildWindow";

                var window = NativeMethods.FindWindow(null, title);

                if (window != IntPtr.Zero)
                {
                    var firstChildren = GetChildWindows(window);

                    foreach (var first in firstChildren)
                    {
                        var windowName = GetCaptionOfWindow(first);
                        var windowClass = GetClassNameOfWindow(first);

                        if (windowClass.Contains(targetClass) &&
                            windowName == name)
                        {
                            return first;
                        }
                    }
                }
            }
            catch
            {
            }

            return IntPtr.Zero;
        }

        public static IntPtr FindGameScreen(string title)
        {
            try
            {
                var window = NativeMethods.FindWindow(null, title);

                if (window != IntPtr.Zero)
                {
                    var firstChildren = GetChildWindows(window);

                    if (firstChildren.Count > 3)
                    {
                        return firstChildren[2];
                    }
                }
            }
            catch
            {
            }

            return IntPtr.Zero;
        }

        public static Window GetOperatorMenuWindow()
        {
            var hWnd = GetWindow(Constants.OperatorWindowName);

            if (hWnd == IntPtr.Zero)
            {
                return null;
            }

            var window = (Window)HwndSource.FromHwnd(hWnd)?.RootVisual;

            return window;
        }

        /// <summary>
        ///     Returns a list of child windows
        /// </summary>
        /// <param name="parent">Parent of the windows to return</param>
        /// <returns>List of child windows</returns>
        private static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            var result = new List<IntPtr>();
            var listHandle = GCHandle.Alloc(result);
            try
            {
                var childProc = new EnumWindowProc(EnumWindow);
                NativeMethods.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                {
                    listHandle.Free();
                }
            }

            return result;
        }

        /// <summary>
        ///     Callback method to be used when enumerating windows.
        /// </summary>
        /// <param name="handle">Handle of the next window</param>
        /// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
        /// <returns>True to continue the enumeration, false to bail</returns>
        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            var gch = GCHandle.FromIntPtr(pointer);
            if (!(gch.Target is List<IntPtr> list))
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }

            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        private static string GetCaptionOfWindow(IntPtr hwnd)
        {
            var caption = "";
            try
            {
                var maxLength = NativeMethods.GetWindowTextLength(hwnd);
                var windowText = new StringBuilder("", maxLength + 5);
                NativeMethods.GetWindowText(hwnd, windowText, maxLength + 2);

                if (!string.IsNullOrEmpty(windowText.ToString()) && !string.IsNullOrWhiteSpace(windowText.ToString()))
                {
                    caption = windowText.ToString();
                }
            }
            catch (Exception ex)
            {
                caption = ex.Message;
            }

            return caption;
        }

        private static string GetClassNameOfWindow(IntPtr hwnd)
        {
            var className = "";
            StringBuilder classText = null;
            try
            {
                var cls_max_length = 1000;
                classText = new StringBuilder("", cls_max_length + 5);
                NativeMethods.GetClassName(hwnd, classText, cls_max_length + 2);

                if (!string.IsNullOrEmpty(classText.ToString()) && !string.IsNullOrWhiteSpace(classText.ToString()))
                {
                    className = classText.ToString();
                }
            }
            catch (Exception ex)
            {
                className = ex.Message;
            }
            finally
            {
                classText = null;
            }

            return className;
        }
    }
}