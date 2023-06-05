namespace Aristocrat.Monaco.G2S.DisableProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Protocol.Common.DisableProvider;

    public class G2SDisableProvider : ProtocolDisableProvider<G2SDisableStates>, IG2SDisableProvider
    {
        /// <inheritdoc />
        protected override Dictionary<G2SDisableStates, DisableData> DisableDataDictionary { get; } =
            new Dictionary<G2SDisableStates, DisableData>
            {
                {
                    G2SDisableStates.CommsOffline, new DisableData(
                        G2S.Constants.VertexOfflineKey,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveDisconnectText))
                },
                {
                    G2SDisableStates.LevelMismatch, new DisableData(
                        G2S.Constants.VertexLevelMismatchKey,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveLevelMismatchText))
                },
                {
                    G2SDisableStates.ProgressiveStateDisabledByHost, new DisableData(
                        G2S.Constants.VertexStateDisabledKey,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveFaultTypes_StateDisabledByHost))
                },
                {
                    G2SDisableStates.ProgressiveValueNotReceived, new DisableData(
                        G2S.Constants.VertexUpdateNotReceivedKey,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveFaultTypes_ProgUpdateTimeout))
                },
                {
                    G2SDisableStates.ProgressiveMeterRollback, new DisableData(
                        G2S.Constants.VertexMeterRollbackKey,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveFaultTypes_MeterRollback))
                },
            };

        /// <inheritdoc />
        protected override ILog Logger { get; } = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Creates the G2SDisableProvider Instance
        /// </summary>
        /// <param name="systemDisableManager">the system disable manager</param>
        /// <param name="messageDisplay">the message display</param>
        public G2SDisableProvider(
            ISystemDisableManager systemDisableManager,
            IMessageDisplay messageDisplay)
            : base(systemDisableManager, messageDisplay) { }

        /// <inheritdoc />
        public async Task OnG2SReconfigured()
        {
            var states = (G2SDisableStates[])Enum.GetValues(typeof(G2SDisableStates));
            var enablingStates = states.Where(
                state =>
                    state != G2SDisableStates.None &&
                    (IsDisableStateActive(state) || IsSoftErrorStateActive(state))).ToArray();
            Logger.Debug($"Clearing disabled states {string.Join(", ", enablingStates)} due to G2S being reconfigured");
            await Enable(enablingStates);
        }
    }
}
