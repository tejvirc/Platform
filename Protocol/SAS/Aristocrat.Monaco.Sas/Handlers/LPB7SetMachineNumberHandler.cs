namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     The handler for LP B7 Set Machine Number handler
    /// </summary>
    public class LPB7SetMachineNumberHandler : ISasLongPollHandler<LongPollSetMachineNumbersResponse, LongPollSetMachineNumbersData>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IBank _bank;

        /// <summary>
        ///     Creates an instance of the LPB7SetMachineNumberHandler class
        /// </summary>
        /// <param name="propertiesManager">For getting the floor and asset change enabled flags, floor location and asset number</param>
        /// <param name="bank">An instance of <see cref="IBank"/></param>
        public LPB7SetMachineNumberHandler(IPropertiesManager propertiesManager, IBank bank)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SetMachineNumbers
        };

        /// <inheritdoc />
        public LongPollSetMachineNumbersResponse Handle(LongPollSetMachineNumbersData data)
        {
            var controlFlags = MachineNumbersControlFlags.None;
            var floorLocationEnabled = _propertiesManager.GetValue(SasProperties.ChangeFloorLocationSupportedKey, true);
            var assetNumberEnabled = _propertiesManager.GetValue(SasProperties.ChangeAssetNumberSupportedKey, false);
            if (floorLocationEnabled)
            {
                controlFlags |= MachineNumbersControlFlags.FloorLocationConfigurable;
                // if floor location string length is 0 then this is just a query for the system floor location
                if (_bank.QueryBalance() == 0 && data.FloorLocation.Length > 0 &&
                    data.FloorLocation.Length <= ApplicationConstants.MaxLocationLength)
                {
                    _propertiesManager.SetProperty(ApplicationConstants.Location, data.FloorLocation);
                }
            }

            if (assetNumberEnabled)
            {
                controlFlags |= MachineNumbersControlFlags.AssetNumberConfigurable;
                // if the asset number is zero then this is just a query for the asset number
                if (_bank.QueryBalance() == 0 && data.AssetNumber != 0)
                {
                    _propertiesManager.SetProperty(ApplicationConstants.MachineId, data.AssetNumber);
                }
            }

            var floorLocation = _propertiesManager.GetValue(ApplicationConstants.Location, string.Empty);
            var assetNumber = _propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);

            return new LongPollSetMachineNumbersResponse(controlFlags, assetNumber, floorLocation);
        }
    }
}
