namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Gaming.Contracts.Session;
    using Params = Aristocrat.G2S.Client.Devices.v21.G2SParametersNames.PlayerDevice;

    public class PlayerDeviceOptions : BaseDeviceOptions
    {
        private readonly IPlayerService _players; 

        public PlayerDeviceOptions(IPlayerService players)
        {
            _players = players ?? throw new ArgumentNullException(nameof(players));
        }

        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.Player;
        }

        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            var options = _players.Options;

            if (optionConfigValues.HasValue(Params.MinimumTheoHoldPercentageParameterName))
            {
                options.MinimumTheoreticalHoldPercentage = optionConfigValues.Int64Value(Params.MinimumTheoHoldPercentageParameterName);
            }

            if (optionConfigValues.HasValue(Params.DecimalPointsParameterName))
            {
                options.DecimalPoints = optionConfigValues.Int32Value(Params.DecimalPointsParameterName);
            }

            if (optionConfigValues.HasValue(Params.EndSessionOnInactivityParameterName))
            {
                options.InactiveSessionEnd = optionConfigValues.BooleanValue(Params.EndSessionOnInactivityParameterName);
            }

            if (optionConfigValues.HasValue(Params.IntervalPeriodParameterName))
            {
                options.IntervalPeriod = TimeSpan.FromMilliseconds(optionConfigValues.Int32Value(Params.IntervalPeriodParameterName));
            }

            if (optionConfigValues.HasValue(Params.GamePlayIntervalParameterName))
            {
                options.GamePlayInterval = optionConfigValues.BooleanValue(Params.GamePlayIntervalParameterName);
            }

            if (optionConfigValues.HasValue(Params.CountBasisParameterName))
            {
                options.CountBasis = optionConfigValues.StringValue(Params.CountBasisParameterName);
            }

            if (optionConfigValues.HasValue(Params.CountDirectionParameterName))
            {
                options.CountDirection =
                   optionConfigValues.StringValue(Params.CountDirectionParameterName) ==
                    t_countDirection.G2S_up.ToString()
                        ? CountDirection.Up
                        : CountDirection.Down;
            }

            if (optionConfigValues.HasValue(Params.BaseTargetParameterName))
            {
                options.BaseTarget = optionConfigValues.Int32Value(Params.BaseTargetParameterName);
            }

            if (optionConfigValues.HasValue(Params.BaseIncrementParameterName))
            {
                options.BaseIncrement = optionConfigValues.Int64Value(Params.BaseIncrementParameterName);
            }

            if (optionConfigValues.HasValue(Params.BaseAwardParameterName))
            {
                options.BaseAward = optionConfigValues.Int32Value(Params.BaseAwardParameterName);
            }

            if (optionConfigValues.HasValue(Params.HotPlayerBasisParameterName))
            {
                options.HotPlayerBasis = optionConfigValues.StringValue(Params.HotPlayerBasisParameterName);
            }

            if (optionConfigValues.HasValue(Params.HotPlayerPeriodParameterName))
            {
                options.HotPlayerPeriod = TimeSpan.FromMilliseconds(optionConfigValues.Int32Value(Params.HotPlayerPeriodParameterName));
            }

            if (optionConfigValues.HasValue(Params.HotPlayerLimit1ParameterName))
            {
                options.HotPlayerLimit1 = optionConfigValues.Int64Value(Params.HotPlayerLimit1ParameterName);
            }

            if (optionConfigValues.HasValue(Params.HotPlayerLimit2ParameterName))
            {
                options.HotPlayerLimit2 = optionConfigValues.Int64Value(Params.HotPlayerLimit2ParameterName);
            }

            if (optionConfigValues.HasValue(Params.HotPlayerLimit3ParameterName))
            {
                options.HotPlayerLimit3 = optionConfigValues.Int64Value(Params.HotPlayerLimit3ParameterName);
            }

            if (optionConfigValues.HasValue(Params.HotPlayerLimit4ParameterName))
            {
                options.HotPlayerLimit4 = optionConfigValues.Int64Value(Params.HotPlayerLimit4ParameterName);
            }

            if (optionConfigValues.HasValue(Params.HotPlayerLimit5ParameterName))
            {
                options.HotPlayerLimit5 = optionConfigValues.Int64Value(Params.HotPlayerLimit5ParameterName);
            }

            _players.Options = options;
        }
    }
}
