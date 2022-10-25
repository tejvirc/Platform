namespace Aristocrat.Monaco.UI.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using Aristocrat.Monaco.Bootstrap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Tests for the WpfWindowLauncher class
    /// </summary>
    [TestClass]
    public class WpfWindowLauncherTests
    {
        private WpfWindowLauncher _target;
        private Thread _uiThread;
        private ManualResetEvent _waitForAppCreated;

        /// <summary>Gets or sets the test context</summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            if (Application.Current == null)
            {
                CreateAppDomain();
            }

            _target = new WpfWindowLauncher();
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            _target.Dispose();

            if (_waitForAppCreated != null)
            {
                _waitForAppCreated.Close();
                _waitForAppCreated = null;
            }
        }

        /// <summary>
        ///     Cleans up after all tests have executed
        /// </summary>
        [ClassCleanup]
        public static void FinalCleanUp()
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual(nameof(WpfWindowLauncher), _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            ICollection<Type> serviceTypes = _target.ServiceTypes;
            Assert.AreEqual(1, serviceTypes.Count);
            Assert.IsTrue(serviceTypes.Contains(typeof(IWpfWindowLauncher)));
        }

        [TestMethod]
        public void InitializeTest()
        {
            _target.Initialize();

            Assert.IsNotNull(Application.Current);
        }

        [TestMethod]
        public void GetWindowTestForUnknownName()
        {
            _target.Initialize();

            Assert.IsNull(_target.GetWindow("badname"));
        }

        [TestMethod]
        public void CreateWindowTestWhenDisposed()
        {
            _target.Initialize();
            _target.Dispose();

            string windowName = "TestWindow";
            _target.CreateWindow<Window>(windowName);

            Assert.AreEqual(0, GetWindowCount());
        }

        [TestMethod]
        public void CreateWindowTestSuccessNotADialog()
        {
            _target.Initialize();

            string windowName = "TestWindow";
            _target.CreateWindow<Window>(windowName);

            Window createdWindow = _target.GetWindow(windowName);

            Assert.IsNotNull(createdWindow);
            Assert.AreEqual(1, GetWindowCount());
            Assert.AreEqual(windowName, GetWindowName(createdWindow));
        }

        [TestMethod]
        public void CreateWindowTestSuccessDialog()
        {
            _target.Initialize();

            string windowName = "TestWindow";

            // The call to create a dialog blocks until the dialog closes.
            // So, do it in a thread.
            bool createWindowReturned = false;
            Action dialogStarter = new Action(
                () =>
                {
                    _target.CreateWindow<Window>(windowName, true);
                    createWindowReturned = true;
                });

            Thread thread = new Thread(new ThreadStart(dialogStarter));
            thread.Start();

            // Give the thread time to create the dialog, but timeout after a reasonable wait.
            DateTime stopWaitingDateTime = DateTime.Now + TimeSpan.FromSeconds(10.0);
            Window createdWindow = null;
            do
            {
                Thread.Sleep(100);
                createdWindow = _target.GetWindow(windowName);
            } while (createdWindow == null && DateTime.Now < stopWaitingDateTime);

            Assert.IsFalse(createWindowReturned);

            Assert.IsNotNull(createdWindow);
            Assert.AreEqual(1, GetWindowCount());
            Assert.AreEqual(windowName, GetWindowName(createdWindow));
        }

        [TestMethod]
        public void CreateWindowTestTwoUniqueWindows()
        {
            _target.Initialize();

            string window1Name = "TestWindow";
            string window2Name = "SecondWindow";
            _target.CreateWindow<Window>(window1Name);
            _target.CreateWindow<Window>(window2Name);

            Assert.AreEqual(2, GetWindowCount());

            Window createdWindow1 = _target.GetWindow(window1Name);
            Assert.IsNotNull(createdWindow1);
            Assert.AreEqual(window1Name, GetWindowName(createdWindow1));

            Window createdWindow2 = _target.GetWindow(window2Name);
            Assert.IsNotNull(createdWindow2);
            Assert.AreEqual(window2Name, GetWindowName(createdWindow2));
        }

        [TestMethod]
        public void CreateWindowTestForDuplicateWindow()
        {
            _target.Initialize();

            string windowName = "TestWindow";
            _target.CreateWindow<Window>(windowName);

            // Try a second time
            bool threwException = false;
            try
            {
                _target.CreateWindow<Window>(windowName);
            }
            catch (ArgumentException)
            {
                threwException = true;
            }

            Assert.IsTrue(threwException);

            // First window should still be fine
            Window createdWindow = _target.GetWindow(windowName);

            Assert.IsNotNull(createdWindow);
            Assert.AreEqual(1, GetWindowCount());
            Assert.AreEqual(windowName, GetWindowName(createdWindow));
        }

        [TestMethod]
        public void GetWindowVisibilityTest()
        {
            _target.Initialize();

            string windowName = "TestWindow";
            _target.CreateWindow<Window>(windowName);

            Assert.AreEqual(Visibility.Visible, _target.GetWindowVisibility(windowName));
            Assert.AreEqual(Visibility.Collapsed, _target.GetWindowVisibility("badname"));
        }

        [TestMethod]
        public void ShowAndHideTest()
        {
            _target.Initialize();

            string windowName = "TestWindow";
            _target.CreateWindow<Window>(windowName);

            _target.Hide(windowName);
            Assert.AreEqual(Visibility.Hidden, _target.GetWindowVisibility(windowName));

            _target.Show(windowName);
            Assert.AreEqual(Visibility.Visible, _target.GetWindowVisibility(windowName));
        }

        [TestMethod]
        public void WindowStateTest()
        {
            _target.Initialize();

            string windowName = "TestWindow";
            _target.CreateWindow<Window>(windowName);

            Assert.AreEqual(WindowState.Normal, _target.GetWindowState(windowName));
            Assert.AreEqual(WindowState.Normal, _target.GetWindowState("badname"));

            _target.SetWindowState(windowName, WindowState.Minimized);
            Assert.AreEqual(WindowState.Minimized, _target.GetWindowState(windowName));
        }

        [TestMethod]
        public void CloseTest()
        {
            _target.Initialize();

            string window1Name = "TestWindow";
            string window2Name = "SecondWindow";
            _target.CreateWindow<Window>(window1Name);
            _target.CreateWindow<Window>(window2Name);

            Assert.IsNotNull(_target.GetWindow(window1Name));

            Window window2 = _target.GetWindow(window2Name);
            Assert.IsNotNull(window2);

            bool window2Closed = false;
            window2.Closed += (sender, eventArgs) => window2Closed = true;

            _target.Close(window2Name);
            Assert.IsTrue(window2Closed);
            Assert.IsNull(_target.GetWindow(window2Name));
        }

        [TestMethod]
        public void DisposeTest()
        {
            _target.Initialize();

            string window1Name = "TestWindow";
            string window2Name = "SecondWindow";
            _target.CreateWindow<Window>(window1Name);
            _target.CreateWindow<Window>(window2Name);

            _target.Dispose();

            // A second dispose should do nothing
            _target.Dispose();
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

        private void CreateAppDomain()
        {
            Assert.IsNull(Application.Current);
            _waitForAppCreated = new ManualResetEvent(false);

            if (_uiThread == null)
            {
                _uiThread = new Thread(
                    () =>
                    {
                        if (Application.Current == null)
                        {
                            var app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };

                            _waitForAppCreated.Set();

                            app.Run();
                        }
                    })
                {
                    Name = "UI Thread",
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };

                _uiThread.SetApartmentState(ApartmentState.STA);
                _uiThread.Start();
                _waitForAppCreated.WaitOne();
            }
        }
    }
}