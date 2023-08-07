namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts.Lobby;
    using Contracts.Models;
    using log4net;

    /// <summary>
    ///     Lobby state queue.
    /// </summary>
    public class LobbyStateQueue : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        //private readonly object _lockObject = new object();
        private readonly List<LobbyState> _stateList = new List<LobbyState>();
        private ReaderWriterLockSlim _queueLock;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyStateQueue"/> class.
        /// </summary>
        public LobbyStateQueue()
        {
            Logger.Debug("LobbyStateQueue constructed.");
            _queueLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _stateList.Add(LobbyState.Startup);
        }

        /// <inheritdoc />
        ~LobbyStateQueue()
        {
            Dispose(false);
        }

        public LobbyState BaseState
        {
            get
            {
                _queueLock?.EnterReadLock();
                try
                {
                    return _stateList[0];
                }
                finally
                {
                    _queueLock?.ExitReadLock();
                }
            }
        }

        public LobbyState TopState => GetTopStateExcluding(null);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void HandleStateTransition(LobbyState source, LobbyState destination)
        {
            Logger.Debug($"HandleStateTransition Source: {source} Destination: {destination}");

            var sourceIsStackable = IsStateStackable(source);
            var destinationIsStackable = IsStateStackable(destination);

            if (!sourceIsStackable && !destinationIsStackable)
            {
                // Resetting the base state
                SetNewBaseState(destination);
            }
            else if (!sourceIsStackable)
            {
                // adding stacked state
                AddStackableState(destination);
            }
            else if (!destinationIsStackable)
            {
                // exiting from a stacked state to a base state
                RemoveStackableState(source);
            }
            else
            {
                // moving from one stacked state to another.  Check for existence to see what is going on
                if (ContainsAny(destination))
                {
                    // we are exiting
                    RemoveStackableState(source);
                }
                else
                {
                    AddStackableState(destination);
                }
            }
        }

        public void SetNewBaseState(LobbyState state)
        {
            Logger.Debug($"SetNewBaseState: {state}");
            _queueLock?.EnterWriteLock();
            try
            {
                _stateList.RemoveAt(0);
                _stateList.Insert(0, state);
            }
            finally
            {
                _queueLock?.ExitWriteLock();
            }
            Logger.Debug(CurrentStateList());
        }

        public void AddStackableState(LobbyState state)
        {
            Debug.Assert(IsStateStackable(state));
            Logger.Debug($"AddStackableState: {state}");
            _queueLock?.EnterWriteLock();
            try
            {
                if (!ContainsAny(state))
                {
                    _stateList.Add(state);
                }
            }
            finally
            {
                _queueLock?.ExitWriteLock();
            }
            Logger.Debug(CurrentStateList());
        }

        public void RemoveStackableState(LobbyState state)
        {
            Debug.Assert(IsStateStackable(state));
            Logger.Debug($"RemoveStackableState: {state}");
            _queueLock?.EnterWriteLock();
            try
            {
                if (ContainsAny(state))
                {
                    _stateList.Remove(state);
                }
            }
            finally
            {
                _queueLock?.ExitWriteLock();
            }
            Logger.Debug(CurrentStateList());
        }

        public void AddFlagState(LobbyState state)
        {
            Debug.Assert(IsStateFlag(state));
            AddStackableState(state);
        }

        public void RemoveFlagState(LobbyState state)
        {
            Debug.Assert(IsStateFlag(state));
            RemoveStackableState(state);
        }

        public static bool IsStateStackable(LobbyState state)
        {
            var stackableAttribute = EnumHelper.GetAttribute<StackableLobbyStateAttribute>(state);
            return stackableAttribute != null;
        }

        public static bool IsStateFlag(LobbyState state)
        {
            var flagAttribute = EnumHelper.GetAttribute<FlagLobbyStateAttribute>(state);
            return flagAttribute != null;
        }

        public bool ContainsAny(params LobbyState[] states)
        {
            if (states == null || states.Length == 0)
            {
                return false;
            }

            _queueLock?.EnterReadLock();
            try
            {
                return _stateList.Any(states.Contains);
            }
            finally
            {
                _queueLock?.ExitReadLock();
            }
        }

        public LobbyState GetTopStateExcluding(LobbyState state)
        {
            return GetTopStateExcluding((LobbyState?)state);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _queueLock?.Dispose();
            }

            _queueLock = null;

            _disposed = true;
        }

        private LobbyState GetTopStateExcluding(LobbyState? state)
        {
            Debug.Assert(!state.HasValue || IsStateStackable(state.Value));
            LobbyState? topState = null;
            _queueLock?.EnterReadLock();
            try
            {
                for (var i = _stateList.Count - 1; i >= 0 && !topState.HasValue; i--)
                {
                    if (!IsStateFlag(_stateList[i]) && (!state.HasValue || _stateList[i] != state.Value)
                    ) //Ignore Flag States
                    {
                        topState = _stateList[i];
                    }
                }

                Debug.Assert(topState.HasValue);
                return topState.Value;
            }
            finally
            {
                _queueLock?.ExitReadLock();
            }
        }

        private string CurrentStateList()
        {
            _queueLock?.EnterReadLock();
            try
            {
                return string.Join(", ", _stateList);
            }

            finally
            {
                _queueLock?.ExitReadLock();
            }
        }
    }
}
