namespace Aristocrat.Monaco.G2S.Options
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Handlers.IdReader;
    using Hardware.Contracts.IdReader;
    using System;
    using System.Collections.Generic;
    using Handlers;
    using g2s = Aristocrat.G2S.Client.Devices.v21.G2SParametersNames;
    using param = Aristocrat.G2S.Client.Devices.v21.G2SParametersNames.IdReaderDevice;

    public class IdReaderDeviceOptions : BaseDeviceOptions
    {
        private readonly IIdReaderProvider _idReaderProvider;

        public IdReaderDeviceOptions(IIdReaderProvider idReaderProvider)
        {
            _idReaderProvider = idReaderProvider ?? throw new ArgumentNullException(nameof(idReaderProvider));
        }

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass.IdReader;
        }

        protected override void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            CheckParameters(device.Id, optionConfigValues);

            var idReader = _idReaderProvider[device.Id];

            if (optionConfigValues.HasValue(g2s.RequiredForPlayParameterName))
            {
                idReader.RequiredForPlay = optionConfigValues.BooleanValue(g2s.RequiredForPlayParameterName);
            }

            if (optionConfigValues.HasValue(param.EgmPhysicallyControlsParameterName))
            {
                //TODO : should we allow setting this value? idReader.IsEgmControlled = 
                optionConfigValues.BooleanValue(param.EgmPhysicallyControlsParameterName);
            }

            if (optionConfigValues.HasValue(param.IdReaderTypeParameterName))
            {
                //TODO : should we allow setting this value? idReader.IdReaderType = 
                optionConfigValues.StringValue(param.IdReaderTypeParameterName).ToReaderType();
            }

            if (optionConfigValues.HasValue(param.IdReaderTrackParameterName))
            {
                idReader.IdReaderTrack = optionConfigValues.Int32Value(param.IdReaderTrackParameterName).ToTrackId();
            }

            if (optionConfigValues.HasValue(param.IdValidMethodParameterName))
            {
                if (Enum.TryParse<t_idValidMethods>(
                    optionConfigValues.StringValue(param.IdValidMethodParameterName),
                    out var method))
                {
                    idReader.ValidationMethod = method.ToValidationMethod();
                }
            }

            if (optionConfigValues.HasValue(param.WaitTimeOutParameterName))
            {
                idReader.WaitTimeout = optionConfigValues.Int32Value(param.WaitTimeOutParameterName);
            }

            if (optionConfigValues.HasValue(param.OffLineValidParameterName))
            {
                idReader.SupportsOfflineValidation = optionConfigValues.BooleanValue(param.OffLineValidParameterName);
            }

            if (optionConfigValues.HasValue(param.ValidTimeOutParameterName))
            {
                idReader.ValidationTimeout = optionConfigValues.Int32Value(param.ValidTimeOutParameterName);
            }

            if (optionConfigValues.HasValue(param.RemovalDelayParameterName))
            {
                idReader.RemovalDelay = optionConfigValues.Int32Value(param.RemovalDelayParameterName);
            }

            if (optionConfigValues.HasValue(param.OffLinePatternParameterName))
            {
                var table = optionConfigValues.GetTableValue(param.OffLinePatternParameterName);

                var patterns = new List<OfflineValidationPattern>();

                foreach (var row in table)
                {
                    if (!row.HasValue(param.IdTypeParameterName))
                    {
                        continue;
                    }

                    var id = row.GetDeviceOptionConfigValue(param.IdTypeParameterName).StringValue();
                    var pattern = row.GetDeviceOptionConfigValue(param.OffLinePatternParameterName).StringValue();

                    patterns.Add(new OfflineValidationPattern(id, pattern));
                }

                idReader.Patterns = patterns;
            }
        }
    }
}
