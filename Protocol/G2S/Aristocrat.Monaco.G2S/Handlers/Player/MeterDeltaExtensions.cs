namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Meters;

    public static class MeterDeltaExtensions
    {
        public static IEnumerable<meterDeltaSubscription> Filter(
            this IEnumerable<meterDeltaHostSubscription> @this,
            IEnumerable<meterSelect> query)
        {
            var result = new List<meterDeltaSubscription>();

            var subList = @this.ToList();

            foreach (var meterSelect in query)
            {
                result.AddRange(
                    subList.Where(
                            s => (meterSelect.deviceClass == DeviceClass.G2S_all || meterSelect.deviceClass == s.deviceClass) &&
                                 (meterSelect.deviceId == DeviceId.All || meterSelect.deviceId == s.deviceId) &&
                                 (meterSelect.meterName == DeviceClass.G2S_all || meterSelect.meterName == s.meterName))
                        .Select(
                            m => new meterDeltaSubscription
                            {
                                deviceClass = m.deviceClass,
                                deviceId = m.deviceId,
                                meterName = m.meterName
                            }));
            }

            return result.Distinct(new MeterSubscriptionComparer());
        }

        public static IEnumerable<meterDeltaHostSubscription> Expand(
            this IEnumerable<meterDeltaHostSubscription> @this,
            IEnumerable<IDevice> devices)
        {
            var result = new List<meterDeltaHostSubscription>();

            var deviceList = devices.ToList();

            foreach (var subscription in @this)
            {
                var deviceMeters = MeterMap.DeviceMeters.Where(
                    m => subscription.deviceClass == DeviceClass.G2S_all || subscription.deviceClass == m.Key);

                foreach (var deviceMeter in deviceMeters)
                {
                    foreach (var device in deviceList.Where(
                        d => d.PrefixedDeviceClass() == deviceMeter.Key &&
                             (d.Id == subscription.deviceId || subscription.deviceId == 0 || subscription.deviceId == DeviceId.All)))
                    {
                        result.AddRange(
                            from meter in deviceMeter.Value
                            where subscription.meterName == DeviceClass.G2S_all ||
                                  meter.Key == subscription.meterName
                            select new meterDeltaHostSubscription
                            {
                                deviceClass = deviceMeter.Key,
                                deviceId = device.Id,
                                meterName = meter.Key
                            });
                    }
                }
            }

            return result.Distinct(new HostSubscriptionComparer());
        }

        #region Comparers

        private class HostSubscriptionComparer : IEqualityComparer<meterDeltaHostSubscription>
        {
            public bool Equals(meterDeltaHostSubscription x, meterDeltaHostSubscription y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return x.deviceClass == y.deviceClass && x.deviceId == y.deviceId && x.meterName == y.meterName;
            }

            public int GetHashCode(meterDeltaHostSubscription obj)
            {
                var deviceClassHash = obj.deviceClass == null ? 0 : obj.deviceClass.GetHashCode();
                var deviceIdHash = obj.deviceId.GetHashCode();
                var meterNameHash = obj.meterName == null ? 0 : obj.meterName.GetHashCode();

                return deviceClassHash ^ deviceIdHash ^ meterNameHash;
            }
        }

        private class MeterSubscriptionComparer : IEqualityComparer<meterDeltaSubscription>
        {
            public bool Equals(meterDeltaSubscription x, meterDeltaSubscription y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return x.deviceClass == y.deviceClass && x.deviceId == y.deviceId && x.meterName == y.meterName;
            }

            public int GetHashCode(meterDeltaSubscription obj)
            {
                var deviceClassHash = obj.deviceClass == null ? 0 : obj.deviceClass.GetHashCode();
                var deviceIdHash = obj.deviceId.GetHashCode();
                var meterNameHash = obj.meterName == null ? 0 : obj.meterName.GetHashCode();

                return deviceClassHash ^ deviceIdHash ^ meterNameHash;
            }
        }

        #endregion
    }
}
