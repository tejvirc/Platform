namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Handlers.OptionConfig.Builders;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class NoteAcceptorDeviceOptions : BaseDeviceOptions
    {
        private const string DenominationsKey = @"Denominations";

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string[] Options =
        {
            OptionConstants.ProtocolOptionsId,
            OptionConstants.ProtocolAdditionalOptionsId,
            OptionConstants.NoteAcceptorOptionsId,
            OptionConstants.NoteAcceptorDataTable
        };

        private static readonly Dictionary<string, OptionDataType> OptionsValues = new Dictionary
                <string, OptionDataType>()
            .AddValues(AddProtocolOptionsTypes())
            .AddValues(AddProtocolOptions3Types())
            .AddValues(AddNoteAcceptorOptionsTypes());

        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IPersistentBlock _denominationsPersistentBlock;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorDeviceOptions" /> class.
        /// </summary>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        /// <param name="persistenceProvider">An <see cref="IPersistenceProvider" /> instance.</param>
        /// <param name="propertiesManager">An <see cref="IPropertiesManager"/></param>
        public NoteAcceptorDeviceOptions(
            IDeviceRegistryService deviceRegistry,
            IPersistenceProvider persistenceProvider,
            IPropertiesManager propertiesManager)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _denominationsPersistentBlock = persistenceProvider?.GetOrCreateBlock(DenominationsKey, PersistenceLevel.Critical)
                                            ?? throw new ArgumentNullException(nameof(persistenceProvider));
        }

        /// <inheritdoc />
        protected override IEnumerable<string> SupportedOptions => Options;

        /// <inheritdoc />
        protected override IReadOnlyDictionary<string, OptionDataType> SupportedValues => OptionsValues;

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.NoteAcceptor;
        }

        /// <inheritdoc />
        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            SetNoteAcceptorStatus(optionConfigValues);

            SetDenominations(optionConfigValues);
        }

        /// <summary>
        ///     Adds the NoteAcceptor device options types.
        /// </summary>
        /// <returns>Defined shared NoteAcceptor device options data types.</returns>
        private static IEnumerable<Tuple<string, OptionDataType>> AddNoteAcceptorOptionsTypes()
        {
            return new List<Tuple<string, OptionDataType>>
            {
                new Tuple<string, OptionDataType>(OptionConstants.NoteAcceptorOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(OptionConstants.NoteAcceptorDataTable, OptionDataType.Complex),
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
                    G2SParametersNames.NoteAcceptor.NoteEnabledParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.NoteAcceptor.VoucherEnabledParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(G2SParametersNames.CurrencyIdParameterName, OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.NoteAcceptor.DenomIdParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.NoteAcceptor.BaseCashableAmountParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.NoteAcceptor.NoteActiveParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.NoteAcceptor.TokenParameterName,
                    OptionDataType.Boolean),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.NoteAcceptor.BasePromoAmountParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.NoteAcceptor.BaseNonCashAmountParameterName,
                    OptionDataType.Integer)
            };
        }

        private static int ParseDenomId(string denomId, long currencyMultiplier)
        {
            if (long.TryParse(denomId, out var denom))
            {
                return (int)(denom / currencyMultiplier);
            }

            throw new ArgumentException("Unsupported denom id.");
        }

        private void SetNoteAcceptorStatus(DeviceOptionConfigValues optionConfigValues)
        {
            var noteAcceptor = _deviceRegistry.GetDevice<INoteAcceptor>();

            var denominations = noteAcceptor.Denominations.ToList();

            if (optionConfigValues.HasValue(G2SParametersNames.NoteAcceptor.NoteEnabledParameterName))
            {
                var noteEnabled =
                    optionConfigValues.BooleanValue(G2SParametersNames.NoteAcceptor.NoteEnabledParameterName);

                _propertiesManager.SetProperty(PropertyKey.NoteIn, noteEnabled);

                if (!noteEnabled)
                {
                    _denominationsPersistentBlock.SetValue(
                        DenominationsKey,
                        new Denominations { Denoms = denominations.ToArray() });

                    denominations.Clear();
                }
                else
                {
                    // In the event the host re-enabled notes but didn't configure the denoms use our stored value, if we have one
                    if (denominations.Count == 0)
                    {
                        if (_denominationsPersistentBlock.GetValue(DenominationsKey, out Denominations recoveredDenoms) && recoveredDenoms.Denoms.Any())
                        {
                            denominations = recoveredDenoms.Denoms.ToList();
                        }
                    }
                }
            }

            if (optionConfigValues.HasValue(G2SParametersNames.NoteAcceptor.VoucherEnabledParameterName))
            {
                _propertiesManager.SetProperty(PropertyKey.VoucherIn, optionConfigValues.BooleanValue(G2SParametersNames.NoteAcceptor.VoucherEnabledParameterName));
            }

            UpdateDenominations(denominations, noteAcceptor);
        }

        private static void UpdateDenominations(List<int> denominations, INoteAcceptor noteAcceptor)
        {
            if (denominations.Count != noteAcceptor.Denominations.Count)
            {
                Logger.Debug($"Setting Note Acceptor Status to {denominations}");

                foreach (var denom in noteAcceptor.GetSupportedNotes())
                {
                    noteAcceptor.UpdateDenom(denom, denominations.Contains(denom));
                }
            }
        }

        private void SetDenominations(DeviceOptionConfigValues configValues)
        {
            var noteAcceptor = _deviceRegistry.GetDevice<INoteAcceptor>();

            var denominations = noteAcceptor.Denominations;

            if (!configValues.HasTableValue(G2SParametersNames.NoteAcceptor.NoteAcceptorDataParameterName))
            {
                return;
            }

            var table = configValues.GetTableValue(G2SParametersNames.NoteAcceptor.NoteAcceptorDataParameterName);

            // There is a possibility that the host won't send us all of the denoms we support,
            //   so we need to keep track of anything that wasn't set and disable it when we're done
            var available = noteAcceptor.GetSupportedNotes();

            foreach (var tableRow in table)
            {
                var denomParameter =
                    tableRow.Values.FirstOrDefault(
                        v => v.ParameterId == G2SParametersNames.NoteAcceptor.DenomIdParameterName);

                var noteActive =
                    tableRow.Values.FirstOrDefault(
                        v => v.ParameterId == G2SParametersNames.NoteAcceptor.NoteActiveParameterName);

                if (denomParameter == null || noteActive == null)
                {
                    continue;
                }

                var denom = ParseDenomId(
                    denomParameter.Value,
                    (long)_propertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier));

                if (noteActive.BooleanValue())
                {
                    denominations.Add(denom);
                }
                else
                {
                    denominations.Remove(denom);
                }

                available.Remove(denom);
            }

            // Disable any denoms that weren't explicitly configured
            foreach (var denom in available)
            {
                denominations.Remove(denom);
            }

            if (denominations.Count == 0)
            {
                // If all notes are currently disabled we're going to make an assumption that the host has set G2S_noteEnabled to false.
                //  In that case, we're going to store the denoms, which will then be used to restore the enabled denominations if needed.
                //  We're doing this to work around the note acceptor service behavior/configuration.  There is no discreet state for accepting notes.
                //  It relies solely on the state of the individual notes.  The one caveat to this is that we're largely dependent on the order of the options.
                _denominationsPersistentBlock.SetValue(
                    DenominationsKey,
                    new Denominations { Denoms = noteAcceptor.Denominations.ToArray() });
            }

            UpdateDenominations(denominations, noteAcceptor);
        }

        [Serializable]
        internal class Denominations
        {
            [JsonProperty]
            public int[] Denoms;
        }
    }
}
