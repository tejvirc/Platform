namespace Aristocrat.Monaco.UI.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Tests for the WpfWindowLauncher class
    /// </summary>
    [TestClass]
    public class WpfWindowLauncherTests
    {
        [TestMethod]
        public void ConstructorTest()
        {
            Application.Current.Dispatcher.Invoke(() => Assert.IsNotNull(new WpfWindowLauncher()));
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual(nameof(WpfWindowLauncher), new WpfWindowLauncher().Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            RunTest(() =>
            {
                ICollection<Type> serviceTypes = new WpfWindowLauncher().ServiceTypes;
                Assert.AreEqual(1, serviceTypes.Count);
                Assert.IsTrue(serviceTypes.Contains(typeof(IWpfWindowLauncher)));
            });
        }

        [TestMethod]
        public void InitializeTest()
        {
            RunTest(() =>
            {
                new WpfWindowLauncher().Initialize();
                Assert.IsNotNull(Application.Current);
            });
        }

        [TestMethod]
        public void GetWindowTestForUnknownName()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();
                Assert.IsNull(target.GetWindow("badname"));
            });
        }

        [TestMethod]
        public void CreateWindowTestWhenDisposed()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();
                target.Dispose();

                string windowName = "TestWindow";
                target.CreateWindow<Window>(windowName);

                Assert.AreEqual(0, GetWindowCount());
            });
        }

        [TestMethod]
        public void CreateWindowTestSuccessNotADialog()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                string windowName = "TestWindow";
                target.CreateWindow<Window>(windowName);

                Window createdWindow = target.GetWindow(windowName);

                Assert.IsNotNull(createdWindow);
                Assert.AreEqual(1, GetWindowCount());
                Assert.AreEqual(windowName, GetWindowName(createdWindow));
            });
        }

        [TestMethod]
        public void CreateWindowTestSuccessDialog()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                const string windowName = "TestWindow";
                target.CreateWindow<Window>(windowName);
                var createdWindow = target.GetWindow(windowName);

                Assert.IsNotNull(createdWindow);
                Assert.AreEqual(1, GetWindowCount());
                Assert.AreEqual(windowName, GetWindowName(createdWindow));
            });
        }

        [TestMethod]
        public void CreateWindowTestTwoUniqueWindows()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                string window1Name = "TestWindow";
                string window2Name = "SecondWindow";
                target.CreateWindow<Window>(window1Name);
                target.CreateWindow<Window>(window2Name);

                Assert.AreEqual(2, GetWindowCount());

                Window createdWindow1 = target.GetWindow(window1Name);
                Assert.IsNotNull(createdWindow1);
                Assert.AreEqual(window1Name, GetWindowName(createdWindow1));

                Window createdWindow2 = target.GetWindow(window2Name);
                Assert.IsNotNull(createdWindow2);
                Assert.AreEqual(window2Name, GetWindowName(createdWindow2));
            });
        }

        [TestMethod]
        public void CreateWindowTestForDuplicateWindow()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                string windowName = "TestWindow";
                target.CreateWindow<Window>(windowName);

                // Try a second time
                bool threwException = false;
                try
                {
                    target.CreateWindow<Window>(windowName);
                }
                catch (ArgumentException)
                {
                    threwException = true;
                }

                Assert.IsTrue(threwException);

                // First window should still be fine
                Window createdWindow = target.GetWindow(windowName);

                Assert.IsNotNull(createdWindow);
                Assert.AreEqual(1, GetWindowCount());
                Assert.AreEqual(windowName, GetWindowName(createdWindow));
            });
        }

        [TestMethod]
        public void GetWindowVisibilityTest()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                string windowName = "TestWindow";
                target.CreateWindow<Window>(windowName);

                Assert.AreEqual(Visibility.Visible, target.GetWindowVisibility(windowName));
                Assert.AreEqual(Visibility.Collapsed, target.GetWindowVisibility("badname"));
            });
        }

        [TestMethod]
        public void ShowAndHideTest()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                string windowName = "TestWindow";
                target.CreateWindow<Window>(windowName);

                target.Hide(windowName);
                Assert.AreEqual(Visibility.Hidden, target.GetWindowVisibility(windowName));

                target.Show(windowName);
                Assert.AreEqual(Visibility.Visible, target.GetWindowVisibility(windowName));
            });
        }

        [TestMethod]
        public void WindowStateTest()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                string windowName = "TestWindow";
                target.CreateWindow<Window>(windowName);

                Assert.AreEqual(WindowState.Normal, target.GetWindowState(windowName));
                Assert.AreEqual(WindowState.Normal, target.GetWindowState("badname"));

                target.SetWindowState(windowName, WindowState.Minimized);
                Assert.AreEqual(WindowState.Minimized, target.GetWindowState(windowName));
            });
        }

        [TestMethod]
        public void CloseTest()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                string window1Name = "TestWindow";
                string window2Name = "SecondWindow";
                target.CreateWindow<Window>(window1Name);
                target.CreateWindow<Window>(window2Name);

                Assert.IsNotNull(target.GetWindow(window1Name));

                Window window2 = target.GetWindow(window2Name);
                Assert.IsNotNull(window2);

                bool window2Closed = false;
                window2.Closed += (sender, eventArgs) => window2Closed = true;

                target.Close(window2Name);
                Assert.IsTrue(window2Closed);
                Assert.IsNull(target.GetWindow(window2Name));
            });
        }

        [TestMethod]
        public void DisposeTest()
        {
            RunTest(() =>
            {
                var target = new WpfWindowLauncher();
                target.Initialize();

                string window1Name = "TestWindow";
                string window2Name = "SecondWindow";
                target.CreateWindow<Window>(window1Name);
                target.CreateWindow<Window>(window2Name);

                target.Dispose();

                // A second dispose should do nothing
                target.Dispose();
            });
        }

        private int GetWindowCount()
        {
            int windowCount = -1;
            Application.Current.Dispatcher.Invoke(
                () => windowCount = Application.Current.Windows.Count,
                DispatcherPriority.Normal,
                CancellationToken.None,
                TimeSpan.FromSeconds(10.0));
            return windowCount;
        }

        private string GetWindowName(Window window)
        {
            string actualName = null;
            window.Dispatcher.Invoke(() => actualName = window.Name);
            return actualName;
        }

        private void RunTest(Action action)
        {
            var newWindowThread = new Thread(new ThreadStart(() =>
            {
                action();
                // start the Dispatcher processing  
                Dispatcher.Run();
            }));

            // set the apartment state  
            newWindowThread.SetApartmentState(ApartmentState.STA);
            // make the thread a background thread  
            newWindowThread.IsBackground = true;
            // start the thread  
            newWindowThread.Start();
        }
    }
}