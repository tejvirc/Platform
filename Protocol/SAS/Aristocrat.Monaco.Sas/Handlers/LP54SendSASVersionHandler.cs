namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     The handler for LP 54 Send SAS versions and game serial number
    /// </summary>
    public class LP54SendSasVersionHandler : ISasLongPollHandler<LongPollSendSasVersionResponse, LongPollData>
    {
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Creates an instance of the LP54SendSASVersionHandler class
        /// </summary>
        public LP54SendSasVersionHandler(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(IPropertiesManager));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendSasVersionAndGameSerial
        };

        /// <inheritdoc />
        public LongPollSendSasVersionResponse Handle(LongPollData data)
        {
            var serialNumber = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            var sasVersion = _propertiesManager.GetProperty(SasProperties.SasVersion, string.Empty).ToString();

            return new LongPollSendSasVersionResponse(sasVersion, serialNumber);
        }
    }
}