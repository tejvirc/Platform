namespace Aristocrat.G2S.Emdi.Extensions
{
    using Client;
    using Protocol.v21ext1b1;
    using System;

    /// <summary>
    /// Extension methods for EGM
    /// </summary>
    public static class EgmExtensions
    {
        /// <summary>
        /// Converts to G2S EGM state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static t_egmStates ToG2S(this EgmState state)
        {
            switch (state)
            {
                case EgmState.Enabled:
                    return t_egmStates.G2S_enabled;
                case EgmState.OperatorMode:
                    return t_egmStates.G2S_operatorMode;
                case EgmState.AuditMode:
                    return t_egmStates.G2S_auditMode;
                case EgmState.OperatorDisabled:
                    return t_egmStates.G2S_operatorDisabled;
                case EgmState.OperatorLocked:
                    return t_egmStates.G2S_operatorLocked;
                case EgmState.TransportDisabled:
                    return t_egmStates.G2S_transportDisabled;
                case EgmState.HostDisabled:
                    return t_egmStates.G2S_hostDisabled;
                case EgmState.EgmDisabled:
                    return t_egmStates.G2S_egmDisabled;
                case EgmState.EgmLocked:
                    return t_egmStates.G2S_egmLocked;
                case EgmState.HostLocked:
                    return t_egmStates.G2S_hostLocked;
                case EgmState.DemoMode:
                    return t_egmStates.G2S_demoMode;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
