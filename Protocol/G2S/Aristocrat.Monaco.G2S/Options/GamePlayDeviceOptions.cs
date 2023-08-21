namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Gaming.Contracts;
    using Handlers.OptionConfig.Builders;

    /// <inheritdoc />
    public class GamePlayDeviceOptions : BaseDeviceOptions
    {
        private static readonly string[] Options =
        {
            OptionConstants.ProtocolOptionsId,
            OptionConstants.ProtocolAdditionalOptionsId,
            OptionConstants.GamePlayOptionsId,
            OptionConstants.GameTypeOptionsId,
            OptionConstants.GameAccessOptionsId,
            OptionConstants.GameAccessibleOptionsId,
            OptionConstants.GameDenomListOptionsId
        };

        private static readonly Dictionary<string, OptionDataType> OptionsValues = new Dictionary
                <string, OptionDataType>()
            .AddValues(AddProtocolOptionsTypes())
            .AddValues(AddProtocolOptions3Types())
            .AddValues(AddGamePlayOptionsTypes());

        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayDeviceOptions" /> class.
        /// </summary>
        /// <param name="gameProvider">The game provider.</param>
        public GamePlayDeviceOptions(IGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        protected override IEnumerable<string> SupportedOptions => Options;

        /// <inheritdoc />
        protected override IReadOnlyDictionary<string, OptionDataType> SupportedValues => OptionsValues;

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.GamePlay;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetValues(optionConfigValues, device.Id);

            SetStandardPlay(optionConfigValues);
            SetTournamentPlay(optionConfigValues);
            SetDenomId(optionConfigValues);
            SetDenomActive(optionConfigValues);

            // TODO: add values for G2S_denomList when available
        }

        /// <summary>
        ///     Adds the GamePlay device options types.
        /// </summary>
        /// <returns>Defined shared GamePlay device options data types.</returns>
        private static IEnumerable<Tuple<string, OptionDataType>> AddGamePlayOptionsTypes()
        {
            return new List<Tuple<string, OptionDataType>>
            {
                new Tuple<string, OptionDataType>(OptionConstants.GamePlayOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(OptionConstants.GameTypeOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(OptionConstants.GameAccessOptionsId, OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(OptionConstants.GameAccessibleOptionsId, OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(OptionConstants.GameDenomListOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.RestartStatusParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.UseDefaultConfigParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.RequiredForPlayParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.ThemeIdParameterName,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.PaytableIdParameterName,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.MaxWagerCreditsParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.ProgAllowedParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.SecondaryAllowedParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.CentralAllowedParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.StandardPlayParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.TournamentPlayParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.DenomIdParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.GamePlayDevice.DenomActiveParameterName,
                    OptionDataType.Boolean)
            };
        }

        private IGameProfile GetGameProfile(int deviveId)
        {
            return _gameProvider.GetGame(deviveId);
        }

        private void SetValues(DeviceOptionConfigValues optionConfigValues, int deviceId)
        {
            var gameOptionConfigValues = new GameOptionConfigValues();

            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.ThemeIdParameterName)
                && MatchProtocolConventionsString(
                    optionConfigValues.StringValue(G2SParametersNames.GamePlayDevice.ThemeIdParameterName)))
            {
                gameOptionConfigValues.ThemeId =
                    optionConfigValues.StringValue(G2SParametersNames.GamePlayDevice.ThemeIdParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.PaytableIdParameterName)
                && MatchProtocolConventionsString(
                    optionConfigValues.StringValue(G2SParametersNames.GamePlayDevice.PaytableIdParameterName)))
            {
                gameOptionConfigValues.PaytableId =
                    optionConfigValues.StringValue(G2SParametersNames.GamePlayDevice.PaytableIdParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.MaxWagerCreditsParameterName)
                && optionConfigValues.Int32Value(G2SParametersNames.GamePlayDevice.MaxWagerCreditsParameterName) > 0)
            {
                gameOptionConfigValues.MaximumWagerCredits =
                    optionConfigValues.Int32Value(G2SParametersNames.GamePlayDevice.MaxWagerCreditsParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.ProgAllowedParameterName))
            {
                gameOptionConfigValues.ProgressiveAllowed =
                    optionConfigValues.BooleanValue(G2SParametersNames.GamePlayDevice.ProgAllowedParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.SecondaryAllowedParameterName))
            {
                gameOptionConfigValues.SecondaryAllowed =
                    optionConfigValues.BooleanValue(G2SParametersNames.GamePlayDevice.SecondaryAllowedParameterName);
            }

            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.CentralAllowedParameterName))
            {
                gameOptionConfigValues.CentralAllowed =
                    optionConfigValues.BooleanValue(G2SParametersNames.GamePlayDevice.CentralAllowedParameterName);
            }

            var profile = GetGameProfile(deviceId);
            if (profile != null)
            {
                _gameProvider.Configure(profile.Id, gameOptionConfigValues);
            }
        }

        private void SetStandardPlay(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.StandardPlayParameterName))
            {
                // ignore?
            }
        }

        private void SetTournamentPlay(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.TournamentPlayParameterName))
            {
                // ignore?
            }
        }

        private void SetDenomId(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.DenomIdParameterName))
            {
                // ignore?
            }
        }

        private void SetDenomActive(DeviceOptionConfigValues optionConfigValues)
        {
            if (optionConfigValues.HasValue(G2SParametersNames.GamePlayDevice.DenomActiveParameterName))
            {
                // ignore?
            }
        }
    }
}