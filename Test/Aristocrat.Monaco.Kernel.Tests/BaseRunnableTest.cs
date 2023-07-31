namespace Aristocrat.Monaco.Kernel.Tests
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;

    /// <summary>
    ///     This is a test class for BaseRunnableTest and is intended
    ///     to contain all BaseRunnableTest Unit Tests
    /// </summary>
    [TestClass]
    public class BaseRunnableTest
    {
        /// <summary>
        ///     A test for State
        /// </summary>
        [TestMethod]
        public void StateTest()
        {
            var testRunnable = new TestRunnableConcrete();

            Assert.AreEqual((object)RunnableState.Uninitialized, testRunnable.RunState);
            Assert.IsFalse(testRunnable.IsDisposed);
        }

        /// <summary>
        ///     A test for Disposed
        /// </summary>
        [TestMethod]
        public void DisposedTest()
        {
            var testRunnable = new TestRunnableConcrete();
            testRunnable.Dispose();

            Assert.IsTrue(testRunnable.IsDisposed);
            Assert.AreEqual((object)RunnableState.Stopped, testRunnable.RunState);
            Assert.IsTrue(GetStopCalledOnConcrete(testRunnable));
        }

        /// <summary>
        ///     A test for Run
        /// </summary>
        [TestMethod]
        public void RunTest()
        {
            var concrete = new TestRunnableConcrete();
            concrete.Initialize();

            Assert.AreEqual((object)RunnableState.Initialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Run();

            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsTrue(concrete.OnRunCalled);
        }

        /// <summary>
        ///     Test that no exceptions are thrown if Run is called after object is disposed
        /// </summary>
        [TestMethod]
        public void RunAfterDisposed()
        {
            var concrete = new TestRunnableConcrete();
            concrete.Initialize();

            Assert.AreEqual((object)RunnableState.Initialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Dispose();

            Assert.IsTrue(concrete.IsDisposed);

            concrete.Run();

            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsFalse(concrete.OnRunCalled);
            Assert.IsTrue(concrete.IsDisposed);
        }

        /// <summary>
        ///     A test for OnStop
        /// </summary>
        [TestMethod]
        public void StopTest()
        {
            var concrete = new TestRunnableConcrete();

            Assert.AreEqual((object)RunnableState.Uninitialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Stop();

            Assert.IsTrue(concrete.OnStopCalled);
            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);
            Assert.IsTrue(GetStopCalledOnConcrete(concrete));
        }

        /// <summary>
        ///     A test for Stop after Disposed
        /// </summary>
        [TestMethod]
        public void StopAfterDisposed()
        {
            var concrete = new TestRunnableConcrete();

            Assert.AreEqual((object)RunnableState.Uninitialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Dispose();

            Assert.IsTrue(concrete.IsDisposed);

            Assert.IsTrue(concrete.OnStopCalled);
            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsTrue(concrete.IsDisposed);
            Assert.IsTrue(GetStopCalledOnConcrete(concrete));

            concrete.OnStopCalled = false;
            concrete.Stop();

            Assert.IsFalse(concrete.OnStopCalled);
            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsTrue(concrete.IsDisposed);
            Assert.IsTrue(GetStopCalledOnConcrete(concrete));
        }

        /// <summary>
        ///     A test for object being disposed before OnStop returns
        /// </summary>
        [TestMethod]
        public void DisposedBeforeOnStopReturns()
        {
            var concrete = new TestRunnableConcrete();
            var stopThread = new Thread(concrete.Stop);
            concrete.ExitOnStop = false;
            stopThread.Start();

            WaitForTrue(
                () => { return concrete.RunState == RunnableState.Stopping; });
            Assert.AreEqual((object)RunnableState.Stopping, concrete.RunState);

            Assert.IsFalse(concrete.IsDisposed);
            Assert.AreEqual((object)RunnableState.Stopping, concrete.RunState);
            Assert.IsTrue(concrete.OnStopCalled);
            Assert.IsTrue(GetStopCalledOnConcrete(concrete));

            var disposeThread = new Thread(concrete.Dispose);
            disposeThread.Start();

            Thread.Sleep(0);

            concrete.ExitOnStop = true;

            WaitForTrue(
                () => { return concrete.RunState == RunnableState.Stopped; });
            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);

            stopThread.Join();
            disposeThread.Join();
        }

        /// <summary>
        ///     A test for Stop
        /// </summary>
        [TestMethod]
        public void StopCalledTwiceTest()
        {
            var concrete = new TestRunnableConcrete();
            concrete.Stop();

            Assert.IsFalse(concrete.IsDisposed);
            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsTrue(concrete.OnStopCalled);

            Assert.IsTrue(GetStopCalledOnConcrete(concrete));

            concrete.OnStopCalled = false;
            concrete.Stop();

            Assert.IsFalse(concrete.OnStopCalled);
        }

        /// <summary>
        ///     A test for OnRun
        /// </summary>
        [TestMethod]
        public void OnRunTest()
        {
            var concrete = new TestRunnableConcrete();
            concrete.Initialize();

            Assert.AreEqual((object)RunnableState.Initialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Run();

            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsTrue(concrete.OnRunCalled);

            concrete.Stop();

            Assert.IsTrue(concrete.OnStopCalled);
            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
        }

        /// <summary>
        ///     A test for OnInitialize
        /// </summary>
        [TestMethod]
        public void InitializeTest()
        {
            var concrete = new TestRunnableConcrete();

            Assert.AreEqual((object)RunnableState.Uninitialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Initialize();

            Assert.AreEqual((object)RunnableState.Initialized, concrete.RunState);
            Assert.IsTrue(concrete.OnInitializeCalled);
            Assert.IsFalse(concrete.IsDisposed);
        }

        /// <summary>
        ///     A test for OnInitialize
        /// </summary>
        [TestMethod]
        public void InitializeTwiceTest()
        {
            var concrete = new TestRunnableConcrete();

            Assert.AreEqual((object)RunnableState.Uninitialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Initialize();

            Assert.AreEqual((object)RunnableState.Initialized, concrete.RunState);
            Assert.IsTrue(concrete.OnInitializeCalled);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.OnInitializeCalled = false;
            concrete.Initialize();

            Assert.AreEqual((object)RunnableState.Initialized, concrete.RunState);
            Assert.IsFalse(concrete.OnInitializeCalled);
            Assert.IsFalse(concrete.IsDisposed);
        }

        /// <summary>
        ///     Test to make sure no exception is thrown if Initialize is called after
        ///     object is disposed.
        /// </summary>
        [TestMethod]
        public void InitializeAfterDisposeTest()
        {
            var concrete = new TestRunnableConcrete();

            Assert.AreEqual((object)RunnableState.Uninitialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Dispose();

            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsTrue(concrete.IsDisposed);

            concrete.Initialize();

            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsFalse(concrete.OnInitializeCalled);
            Assert.IsTrue(concrete.IsDisposed);
        }

        /// <summary>
        ///     Test to make sure no exception is thrown if the object is disposed
        ///     before the OnInitialize method returns and the state is changed
        /// </summary>
        [TestMethod]
        public void DisposeBeforeInitializationFinishes()
        {
            var concrete = new TestRunnableConcrete();

            Assert.AreEqual((object)RunnableState.Uninitialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.ExitOnInitializeCall = false;
            var initializeThread = new Thread(concrete.Initialize);
            initializeThread.Start();

            WaitForTrue(
                () => { return concrete.RunState == RunnableState.Initializing; });
            Assert.AreEqual((object)RunnableState.Initializing, concrete.RunState);

            WaitForTrue(
                () => { return concrete.OnInitializeCalled; });
            Assert.IsTrue(concrete.OnInitializeCalled);

            concrete.Dispose();

            Assert.IsTrue(concrete.IsDisposed);

            concrete.ExitOnInitializeCall = true;
            initializeThread.Join();

            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsTrue(concrete.OnStopCalled);
            Assert.IsTrue(concrete.IsDisposed);
        }

        /// <summary>
        ///     A test for getting the state of the Runnable after the lock is disposed
        /// </summary>
        [TestMethod]
        public void StateLockDisposedTest()
        {
            var concrete = new TestRunnableConcrete();

            var fieldInfo = typeof(BaseRunnable).GetField("_stateLock", BindingFlags.Instance | BindingFlags.NonPublic);
            var stateLock = (ReaderWriterLockSlim)fieldInfo.GetValue(concrete);

            stateLock.Dispose();

            Assert.AreEqual((object)RunnableState.Uninitialized, concrete.RunState);
        }

        /// <summary>
        ///     A test for when the Runnable is already running and Run is called
        /// </summary>
        [TestMethod]
        public void AlreadyRunningTest()
        {
            var concrete = new TestRunnableConcrete();
            concrete.Initialize();

            Assert.AreEqual((object)RunnableState.Initialized, concrete.RunState);
            Assert.IsFalse(concrete.IsDisposed);

            concrete.Run();

            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsTrue(concrete.OnRunCalled);

            concrete.OnRunCalled = false;

            // Make sure an exception isn't thrown and that the Runnable is still running
            concrete.Run();

            Assert.AreEqual((object)RunnableState.Stopped, concrete.RunState);
            Assert.IsFalse(concrete.OnRunCalled);
        }

        /// <summary>
        ///     Gets the private BaseRunnable.stopCalled boolean value provided a TestRunnableConcrete
        /// </summary>
        /// <param name="concrete">The TestRunnableConcrete to get the value of stopCalled for</param>
        /// <returns>The stopCalled value</returns>
        private static bool GetStopCalledOnConcrete(TestRunnableConcrete concrete)
        {
            var stopCalledField =
                typeof(BaseRunnable).GetField("_stopCalled", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = stopCalledField.GetValue(concrete) as bool?;
            if (result.HasValue)
            {
                return result.Value;
            }
            else
                throw new InvalidOperationException("Failure to access field in " + nameof(GetStopCalledOnConcrete));
        }

        /// <summary>
        ///     Executes the check until it is true or a timeout occurs (5 seconds)
        /// </summary>
        /// <param name="check">The check to execute</param>
        private static void WaitForTrue(ConditionCheck check)
        {
            WaitForTrue(check, 5000);
        }

        /// <summary>
        ///     Executes the check until it is true or a timeout occurs
        /// </summary>
        /// <param name="check">The check to execute</param>
        /// <param name="timeout">The timeout</param>
        private static void WaitForTrue(ConditionCheck check, int timeout)
        {
            var startTime = DateTime.UtcNow;
            double elapsedTime = 0;
            while (!check() && (elapsedTime < timeout))
            {
                Thread.Sleep(100);

                elapsedTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            }
        }

        /// <summary>
        ///     A delegate representing a check of some condition
        /// </summary>
        /// <returns>True if the condition is met, false otherwise</returns>
        private delegate bool ConditionCheck();
    }
}