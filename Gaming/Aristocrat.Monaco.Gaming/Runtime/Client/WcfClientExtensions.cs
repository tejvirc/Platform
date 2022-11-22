namespace Aristocrat.Monaco.Gaming.Runtime.Client
{
    using System;
    using GDKRuntime.Contract;
    using OutcomeType = Contracts.Central.OutcomeType;

    /// <summary>
    ///     Provides basic mapping between internal and GDKRuntime.Contract enums and data contracts
    /// </summary>
    public static class WcfClientExtensions
    {
        public static GDKRuntime.Contract.RuntimeState ToGdkRuntimeState(this RuntimeState @this)
        {
            switch (@this)
            {
                case RuntimeState.Initialization:
                    return GDKRuntime.Contract.RuntimeState.Initialization;
                case RuntimeState.Configuration:
                    return GDKRuntime.Contract.RuntimeState.Configuration;
                case RuntimeState.Configured:
                    return GDKRuntime.Contract.RuntimeState.Configured;
                case RuntimeState.Loading:
                    return GDKRuntime.Contract.RuntimeState.Loading;
                case RuntimeState.Recovery:
                    return GDKRuntime.Contract.RuntimeState.Recovery;
                case RuntimeState.Replay:
                    return GDKRuntime.Contract.RuntimeState.Replay;
                case RuntimeState.Pause:
                    return GDKRuntime.Contract.RuntimeState.Pause;
                case RuntimeState.RenderOnly:
                    return GDKRuntime.Contract.RuntimeState.RenderOnly;
                case RuntimeState.Running:
                    return GDKRuntime.Contract.RuntimeState.Running;
                case RuntimeState.LanguageUpdate:
                    return GDKRuntime.Contract.RuntimeState.LanguageUpdate;
                case RuntimeState.Shutdown:
                    return GDKRuntime.Contract.RuntimeState.Shutdown;
                case RuntimeState.Restart:
                    return GDKRuntime.Contract.RuntimeState.Restart;
                case RuntimeState.Abort:
                    return GDKRuntime.Contract.RuntimeState.Abort;
                case RuntimeState.Error:
                    return GDKRuntime.Contract.RuntimeState.Error;
                case RuntimeState.Reconfigure:
                    return GDKRuntime.Contract.RuntimeState.Reconfigure;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static RuntimeState ToRuntimeState(this GDKRuntime.Contract.RuntimeState @this)
        {
            switch (@this)
            {
                case GDKRuntime.Contract.RuntimeState.Abort:
                    return RuntimeState.Abort;
                case GDKRuntime.Contract.RuntimeState.Error:
                    return RuntimeState.Error;
                case GDKRuntime.Contract.RuntimeState.Initialization:
                    return RuntimeState.Initialization;
                case GDKRuntime.Contract.RuntimeState.Configuration:
                    return RuntimeState.Configuration;
                case GDKRuntime.Contract.RuntimeState.Configured:
                    return RuntimeState.Configured;
                case GDKRuntime.Contract.RuntimeState.Loading:
                    return RuntimeState.Loading;
                case GDKRuntime.Contract.RuntimeState.Recovery:
                    return RuntimeState.Recovery;
                case GDKRuntime.Contract.RuntimeState.Replay:
                    return RuntimeState.Replay;
                case GDKRuntime.Contract.RuntimeState.Pause:
                    return RuntimeState.Pause;
                case GDKRuntime.Contract.RuntimeState.RenderOnly:
                    return RuntimeState.RenderOnly;
                case GDKRuntime.Contract.RuntimeState.Running:
                    return RuntimeState.Running;
                case GDKRuntime.Contract.RuntimeState.LanguageUpdate:
                    return RuntimeState.LanguageUpdate;
                case GDKRuntime.Contract.RuntimeState.Shutdown:
                    return RuntimeState.Shutdown;
                case GDKRuntime.Contract.RuntimeState.Restart:
                    return RuntimeState.Restart;
                case GDKRuntime.Contract.RuntimeState.Reconfigure:
                    return RuntimeState.Reconfigure;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static BeginGameRoundState ToGdkBeginGameRoundState(this BeginGameRoundResult @this)
        {
            switch (@this)
            {
                case BeginGameRoundResult.Success:
                    return BeginGameRoundState.Success;
                case BeginGameRoundResult.Failed:
                    return BeginGameRoundState.Failed;
                case BeginGameRoundResult.TimedOut:
                    return BeginGameRoundState.TimedOut;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static BeginGameRoundResult ToBeginGameRoundResult(this BeginGameRoundState @this)
        {
            switch (@this)
            {
                case BeginGameRoundState.Success:
                    return BeginGameRoundResult.Success;
                case BeginGameRoundState.Failed:
                    return BeginGameRoundResult.Failed;
                case BeginGameRoundState.TimedOut:
                    return BeginGameRoundResult.TimedOut;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static GDKRuntime.Contract.OutcomeType ToGdkOutcomeType(this OutcomeType @this)
        {
            switch (@this)
            {
                case OutcomeType.Standard:
                    return GDKRuntime.Contract.OutcomeType.Standard;
                case OutcomeType.Progressive:
                    return GDKRuntime.Contract.OutcomeType.Progressive;
                case OutcomeType.Fractional:
                    return GDKRuntime.Contract.OutcomeType.Fractional;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static OutcomeType ToOutcomeType(this GDKRuntime.Contract.OutcomeType @this)
        {
            switch (@this)
            {
                case GDKRuntime.Contract.OutcomeType.Standard:
                    return OutcomeType.Standard;
                case GDKRuntime.Contract.OutcomeType.Progressive:
                    return OutcomeType.Progressive;
                case GDKRuntime.Contract.OutcomeType.Fractional:
                    return OutcomeType.Fractional;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }
    }
}