namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Protocol.v21;

    /// <summary>
    /// Event report configuration structure.
    /// </summary>
    public struct EventReportConfig
    {
        /// <summary>
        ///     SendDeviceStatus flag.
        /// </summary>
        public readonly bool SendDeviceStatus;

        /// <summary>
        ///     SendClassMeters flag.
        /// </summary>
        public readonly bool SendClassMeters;

        /// <summary>
        ///     SendDeviceMeters flag.
        /// </summary>
        public readonly bool SendDeviceMeters;

        /// <summary>
        ///     SendTransaction flag.
        /// </summary>
        public readonly bool SendTransaction;

        /// <summary>
        ///     SendUpdatableMeters flag.
        /// </summary>
        public readonly bool SendUpdatableMeters;

        /// <summary>
        ///     EventPersist flag.
        /// </summary>
        public readonly bool EventPersist;

        /// <summary>
        ///     DeviceClass.
        /// </summary>
        public readonly string DeviceClass;

        /// <summary>
        ///     DeviceId.
        /// </summary>
        public readonly int DeviceId;

        /// <summary>
        ///     ForcedPersist flag.
        /// </summary>
        public readonly bool ForcedPersist;

        /// <summary>
        /// Event Report Config.
        /// </summary>
        /// <param name="sendDeviceStatus">Flag.</param>
        /// <param name="sendClassMeters">Flag.</param>
        /// <param name="sendDeviceMeters">Flag.</param>
        /// <param name="sendTransaction">Flag.</param>
        /// <param name="sendUpdatableMeters"></param>
        /// <param name="eventPersist">Flag.</param>
        /// <param name="deviceClass">Flag.</param>
        /// <param name="deviceId"></param>
        /// <param name="forcedPersist">Flag</param>
        public EventReportConfig(
            bool sendDeviceStatus,
            bool sendClassMeters,
            bool sendDeviceMeters,
            bool sendTransaction,
            bool sendUpdatableMeters,
            bool eventPersist,
            string deviceClass,
            int deviceId,
            bool forcedPersist)
        {
            SendDeviceStatus = sendDeviceStatus;
            SendClassMeters = sendClassMeters;
            SendDeviceMeters = sendDeviceMeters;
            SendTransaction = sendTransaction;
            SendUpdatableMeters = sendUpdatableMeters;
            EventPersist = eventPersist;
            DeviceClass = deviceClass;
            DeviceId = deviceId;
            ForcedPersist = forcedPersist;
        }
    }

    /// <summary>
    ///     General event handler extensions for the v21 schema.
    /// </summary>
    public static class EventHandlerExtensions
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> Serializers =
            new ConcurrentDictionary<Type, XmlSerializer>();

        /// <summary>
        ///     Creates a deviceList for a IEventHandlerDevice that can be used in an event report.
        /// </summary>
        /// <param name="this">The IEventHandlerDevice device.</param>
        /// <returns>An event report compatible deviceList.</returns>
        public static deviceList1 DeviceList(this IEventHandlerDevice @this)
        {
            var status = new eventHandlerStatus
            {
                configurationId = @this.ConfigurationId,
                egmEnabled = @this.Enabled,
                hostEnabled = @this.HostEnabled,
                eventHandlerOverflow = @this.Overflow,
                configComplete = @this.ConfigComplete
            };

            if (@this.ConfigDateTime != default(DateTime))
            {
                status.configDateTime = @this.ConfigDateTime;
                status.configDateTimeSpecified = true;
            }

            return @this.DeviceList(status);
        }

        /// <summary>
        /// Gets the readable event event description.
        /// </summary>
        /// <param name="eventCode">Event code.</param>
        /// <returns>Event code text</returns>
        public static string GetEventText(string eventCode)
        {
            var field = typeof(EventCode).GetField(eventCode);

            var attribute =
                field?.GetCustomAttributes(true).FirstOrDefault(a => a is DisplayAttribute) as DisplayAttribute;

            return attribute?.Description;
        }

        /// <summary>
        ///     Parse XML string to type.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="xml">XML string.</param>
        /// <returns>Parsed string to type.</returns>
        public static T ParseXml<T>(string xml)
            where T : class
        {
            using (TextReader reader = new StringReader(xml))
            {
                return (T)GetSerializer<T>().Deserialize(reader);
            }
        }

        /// <summary>
        ///     Convert type into string.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="class">Object to convert.</param>
        /// <returns>XML string.</returns>
        public static string ToXml<T>(T @class)
            where T : class
        {
            if (@class == null)
            {
                return string.Empty;
            }

            using (var writer = new StringWriter())
            {
                var serializer = GetSerializer<T>();
                serializer.Serialize(writer, @class);
                return writer.ToString();
            }
        }

        private static XmlSerializer GetSerializer<T>()
        {
            return Serializers.GetOrAdd(typeof(T), _ =>
            {
                var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(T))
                    .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
                return new XmlSerializer(typeof(T), theXmlRootAttribute ?? new XmlRootAttribute(nameof(T)));
            });
        }
    }
}
