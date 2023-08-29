namespace Aristocrat.Monaco.Protocol.Common.DisableProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Kernel;
    using log4net;

    public abstract class ProtocolDisableProvider<TDisableStates> : IProtocolDisableProvider<TDisableStates> where TDisableStates : Enum
    {
        protected internal abstract Dictionary<TDisableStates, DisableData> DisableDataDictionary { get; }

        protected internal abstract ILog Logger { get; }

        protected internal ISystemDisableManager SystemDisableManager { get; }
        protected internal IMessageDisplay MessageDisplay { get; }

        protected internal readonly object _lockObject = new object();
        private TDisableStates _disableState;
        private TDisableStates _softErrorState;

        /// <summary>
        ///     Creates the ProtocolDisableProvider Instance
        /// </summary>
        /// <param name="inSystemDisableManager">the system disable manager</param>
        /// <param name="inMessageDisplay">the message display</param>
        protected ProtocolDisableProvider(
            ISystemDisableManager inSystemDisableManager,
            IMessageDisplay inMessageDisplay)
        {
            SystemDisableManager = inSystemDisableManager ?? throw new ArgumentNullException(nameof(inSystemDisableManager));
            MessageDisplay = inMessageDisplay ?? throw new ArgumentNullException(nameof(inMessageDisplay));
        }

        protected internal TDisableStates DisableState
        {
            get
            {
                lock (_lockObject)
                {
                    return _disableState;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _disableState = value;
                }
            }
        }

        protected internal TDisableStates SoftErrorState
        {
            get
            {
                lock (_lockObject)
                {
                    return _softErrorState;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _softErrorState = value;
                }
            }
        }

        public virtual async Task Disable(SystemDisablePriority priority, params TDisableStates[] states)
        {
            var disableData = states.Where(
                    x => !x.Equals(default(TDisableStates)) && !IsDisableStateActive(x) && DisableDataDictionary.ContainsKey(x))
                .ToList();
            if (!disableData.Any())
            {
                return;
            }

            foreach (var state in disableData)
            {
                if (IsSoftErrorStateActive(state))
                {
                    // Clear soft error if we are wanting to lockup this state now
                    await Enable(state);
                }

                CreateLockup(priority, state);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual async Task Disable(SystemDisablePriority priority, TDisableStates state, bool isLockup)
        {
            if (state.Equals(default(TDisableStates)) || IsAnyStateActive(DisableState, state) ||
                IsSoftErrorStateActive(state) && !isLockup)
            {
                return;
            }

            if (isLockup)
            {
                if (IsSoftErrorStateActive(state))
                {
                    // Clear soft error if we are wanting to lockup this state now
                    await Enable(state);
                }

                CreateLockup(priority, state);
            }
            else
            {
                var disableData = DisableDataDictionary[state];
                MessageDisplay.DisplayMessage(
                    new DisplayableMessage(
                        disableData.Message,
                        DisplayableMessageClassification.SoftError,
                        DisplayableMessagePriority.Normal,
                        disableData.DisableGuid));
                SoftErrorState = SetFlag(SoftErrorState, state);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual async Task Enable(params TDisableStates[] states)
        {
            var disableData = states.Where(
                    x => !x.Equals(default(TDisableStates)) && (IsDisableStateActive(x) || IsSoftErrorStateActive(x)) &&
                         DisableDataDictionary.ContainsKey(x))
                .ToList();
            if (!disableData.Any())
            {
                return;
            }

            foreach (var state in disableData)
            {
                var data = DisableDataDictionary[state];
                if (IsAnyStateActive(SoftErrorState, state))
                {
                    MessageDisplay.RemoveMessage(data.DisableGuid);
                    SoftErrorState = UnsetFlag(SoftErrorState, state);
                }
                else
                {
                    Logger.Debug(
                        $"Requesting the SystemDisableManager to clear disable state: {states}, guid: {data.DisableGuid}");
                    SystemDisableManager.Enable(data.DisableGuid);
                    ClearDisableState(state);
                }
            }

            await Task.CompletedTask;
        }
        /// <inheritdoc />
        public virtual bool IsDisableStateActive(TDisableStates checkState) => IsAnyStateActive(DisableState, checkState);

        /// <inheritdoc />
        public virtual bool IsSoftErrorStateActive(TDisableStates checkState) => IsAnyStateActive(SoftErrorState, checkState);
        protected internal virtual void CreateLockup(SystemDisablePriority priority, TDisableStates state)
        {
            var data = DisableDataDictionary[state];
            Logger.Debug(
                $"Requesting the disable with state={state} guid={data.DisableGuid} reason={data.Message} priority={priority}");

            SystemDisableManager.Disable(data.DisableGuid, priority, data.Message);
            SetDisableState(state);
        }

        protected internal void SetDisableState(TDisableStates state)
        {
            if (IsDisableStateActive(state))
            {
                return;
            }

            Logger.Debug($"Setting disable state: {state}");
            DisableState = SetFlag(DisableState, state);
        }

        protected internal void ClearDisableState(TDisableStates state)
        {
            if (!IsDisableStateActive(state))
            {
                return;
            }

            Logger.Debug($"Clearing disable state: {state}");
            DisableState = UnsetFlag(DisableState, state);
        }

        private static TDisableStates SetFlag(TDisableStates flagStates, TDisableStates flagToSet)
        {
            //Generic types aren't allowed to use the | operator directly,
            //even though the generic type is restricted to Enum
            //This casts the incoming enums to ints, ors them, then casts back to the original type.
            //This behavior may be natively supported directly in C# 11, eliminating this need. 
            return (TDisableStates)Enum.ToObject(typeof(TDisableStates), (int)(object)flagStates | (int)(object)flagToSet);
        }

        private static TDisableStates UnsetFlag(TDisableStates flagStates, TDisableStates flagToUnset)
        {
            //Generic types aren't allowed to use the & operator directly,
            //even though the generic type is restricted to Enum
            //This casts the incoming enums to ints, negates the flag to unset, ands them, then casts back to the original type.
            //This behavior may be natively supported directly in C# 11, eliminating this need. 
            return (TDisableStates)Enum.ToObject(typeof(TDisableStates), (int)(object)flagStates & ~(int)(object)flagToUnset);
        }

        protected internal static bool IsAnyStateActive(TDisableStates currentStates, params TDisableStates[] checkStates)
        {
            return checkStates.Any(x => currentStates.HasFlag(x));
        }
        protected internal class DisableData
        {
            /// <summary>
            ///     Initializes a new instance of the DisableData class.
            /// </summary>
            /// <param name="guid">The guid used when disabling/enabling</param>
            /// <param name="message">The message to display on screen when disabled</param>
            public DisableData(Guid guid, Func<string> message)
            {
                DisableGuid = guid;
                Message = message;
            }

            public Guid DisableGuid { get; }

            public Func<string> Message { get; }
        }
    }
}
