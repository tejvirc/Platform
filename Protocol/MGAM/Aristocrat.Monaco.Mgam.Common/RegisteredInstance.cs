namespace Aristocrat.Monaco.Mgam.Common
{
    using System;
    using Aristocrat.Mgam.Client;
    using MVVM.Model;

    /// <summary>
    ///     The instance registration information.
    /// </summary>
    public class RegisteredInstance : BaseNotify
    {
        private DateTime _timestamp;
        private string _address;
        private int _deviceId;
        private int _instanceId;
        private int _siteId;
        private string _description;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisteredInstance"/> class.
        /// </summary>
        /// <param name="info"></param>
        public RegisteredInstance(InstanceInfo info)
        {
            _timestamp = info.TimeStamp;
            _address = info.ConnectionString;
            _deviceId = info.DeviceId;
            _instanceId = info.InstanceId;
            _siteId = info.SiteId;
            _description = info.Description;
        }

        /// <summary>
        ///     Gets or sets the VLT registration date/time.
        /// </summary>
        public DateTime Timestamp
        {
            get => _timestamp;

            set => SetProperty(ref _timestamp, value, nameof(Timestamp));
        }

        /// <summary>
        ///     Gets or sets the VLT service address.
        /// </summary>
        public string Address
        {
            get => _address;

            set => SetProperty(ref _address, value, nameof(Address));
        }

        /// <summary>
        ///     Gets or sets the VLT device name.
        /// </summary>
        public int DeviceId
        {
            get => _deviceId;

            set => SetProperty(ref _deviceId, value, nameof(DeviceId));
        }

        /// <summary>
        ///     Gets or sets the instance ID.
        /// </summary>
        public int InstanceId
        {
            get => _instanceId;

            set => SetProperty(ref _instanceId, value, nameof(InstanceId));
        }

        /// <summary>
        ///     Gets or sets the site ID.
        /// </summary>
        public int SiteId
        {
            get => _siteId;

            set => SetProperty(ref _siteId, value, nameof(SiteId));
        }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => _description;

            set => SetProperty(ref _description, value, nameof(Description));
        }
    }
}
