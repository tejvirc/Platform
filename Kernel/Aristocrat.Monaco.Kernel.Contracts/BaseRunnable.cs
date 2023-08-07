namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Reflection;
    using System.Threading;
    using log4net;

    /// <summary>
    ///     A class for using Runnable behavior but abstracting the details of the state
    ///     management. It allows a developer to focus on their Initialize, Run and Stop
    ///     behavior instead of managing the Runnable state.
    /// </summary>
    public abstract class BaseRunnable : IRunnable, IDisposable
    {
        
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

        private readonly ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);


        private RunnableState _runState;

        private volatile bool _stopCalled;

        private Thread _runningThread;

        /// <summary>
        ///     Gets or sets a value indicating whether
        ///     this object has been disposed or not
        /// </summary>
        protected bool Disposed { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public RunnableState RunState
        {
            get
            {
                try
                {
                    _stateLock.EnterReadLock();

                    return _runState;
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.Warn(ex);
                }
                finally
                {
                    if (_stateLock.IsReadLockHeld)
                    {
                        _stateLock.ExitReadLock();
                    }
                }

                return _runState;
            }

            set
            {
                try
                {
                    _stateLock.EnterWriteLock();

                    _runState = value;
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.Warn(ex);

                    _runState = value;
                }
                finally
                {
                    if (_stateLock.IsWriteLockHeld)
                    {
                        _stateLock.ExitWriteLock();
                    }
                }
            }
        }

        /// <inheritdoc />
        public TimeSpan Timeout { get;  set; } = DefaultTimeout;

        /// <inheritdoc />
        public void Initialize()
        {
            try
            {
                _stateLock.EnterWriteLock();

                if (RunState == RunnableState.Uninitialized)
                {
                    RunState = RunnableState.Initializing;
                }
                else
                {
                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                Logger.Warn("Attempting to Initialize after being disposed");
                return;
            }
            finally
            {
                if (_stateLock.IsWriteLockHeld)
                {
                    _stateLock.ExitWriteLock();
                }
            }

            OnInitialize();

            try
            {
                _stateLock.EnterWriteLock();

                if (RunState == RunnableState.Initializing)
                {
                    RunState = RunnableState.Initialized;
                }
            }
            catch (ObjectDisposedException)
            {
                Logger.Warn("Attempting to set state in Initialize after being disposed");
            }
            finally
            {
                if (_stateLock.IsWriteLockHeld)
                {
                    _stateLock.ExitWriteLock();
                }
            }
        }

        /// <inheritdoc />
        public void Run()
        {
            try
            {
                _stateLock.EnterWriteLock();

                if (RunState == RunnableState.Initialized)
                {
                    RunState = RunnableState.Running;
                }
                else
                {
                    Logger.Warn($"Run called on {GetType()} and its state is not Initialized, it is {_runState}");

                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                Logger.Warn("Attempting to Run after being disposed");
                return;
            }
            finally
            {
                if (_stateLock.IsWriteLockHeld)
                {
                    _stateLock.ExitWriteLock();
                }
            }

            _runningThread = Thread.CurrentThread;

            OnRun();

            RunState = RunnableState.Stopped;
        }

        /// <inheritdoc />
        public void Stop()
        {
            // Set the previous state to the current state outside the try/catch
            // in case something happens and we can't set it there we at least
            // have the previous state at this moment.
            var previousState = RunState;

            try
            {
                _stateLock.EnterWriteLock();

                if (_stopCalled)
                {
                    return;
                }

                // If the current state is Running then after OnStop
                // returns we won't update the state to Stopped. Instead
                // we wait for the OnRun method to return which sets the state
                // to Stopped when the Run method exits. But, if the state
                // is not Running then when OnStop returns the Runnable is
                // in fact Stopped and the state needs to reflect that.
                previousState = RunState;

                _stopCalled = true;
                if (RunState != RunnableState.Stopped)
                {
                    RunState = RunnableState.Stopping;
                }
            }
            catch (ObjectDisposedException)
            {
                Logger.Warn("Attempting to Stop after being disposed");
                return;
            }
            finally
            {
                if (_stateLock.IsWriteLockHeld)
                {
                    _stateLock.ExitWriteLock();
                }
            }

            OnStop();

            if (previousState != RunnableState.Running)
            {
                RunState = RunnableState.Stopped;
            }

            if (_runningThread != null && _runningThread != Thread.CurrentThread)
            {
                if (!_runningThread.Join(DefaultTimeout.Milliseconds))
                {
                    Logger.Error($"Thread Join failed : {_runningThread.GetType()}");
                }
            }
        }

        /// <summary>
        ///     Disposes of this IDisposable
        /// </summary>
        /// <param name="disposing">Whether the object is being disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Disposed = true;

                if (disposing)
                {
                    Stop();
                    _stateLock.Dispose();
                }
            }
        }

        /// <summary>
        ///     Implemented by derived classes to setup their run state
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        ///     Implemented by derived classes to execute behavior for the Runnable. It is assumed
        ///     that when this method exits the Runnable is then in the Stopped state and Stop will
        ///     be called.
        /// </summary>
        protected abstract void OnRun();

        /// <summary>
        ///     Implemented by derived classes to execute any necessary custom stop behavior
        /// </summary>
        protected abstract void OnStop();
    }
}
