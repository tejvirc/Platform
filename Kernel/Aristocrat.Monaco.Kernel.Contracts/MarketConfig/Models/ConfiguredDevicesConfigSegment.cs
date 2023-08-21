//------------------------------------------------------------------------------
// <auto-generated>
// This file was automatically generated by a tool.
// Schema Format Version: 1.3
// Generator Version: 2.5.0.0
//
// DO NOT MODIFY THIS FILE MANUALLY.
// Changes to this file may cause incorrect behavior and will be overwritten.
// </auto-generated>
//------------------------------------------------------------------------------
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Aristocrat.Monaco.Kernel.MarketConfig.Models.ConfiguredDevices
{
    /// <summary>
    /// Default Devices Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class DefaultDevicesFieldset
    {
        /// <summary>
        /// Name
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public String Name { get; set; }

        /// <summary>
        /// Enabled
        /// </summary>
        [JsonProperty(PropertyName = "enabled", Required = Required.Always)]
        public Boolean Enabled { get; set; }
    }

    /// <summary>
    /// Included Devices Fieldset Component
    /// Access this field using the parent segment class
    /// </summary>
    public class IncludedDevicesFieldset
    {
        /// <summary>
        /// Name
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public String Name { get; set; }

        /// <summary>
        /// Enabled
        /// </summary>
        [JsonProperty(PropertyName = "enabled", Required = Required.Always)]
        public Boolean Enabled { get; set; }
    }

    /// <summary>
    /// Market Jurisdiction Configuration for the Configured Devices segment
    /// </summary>
    [MarketConfigSegment("configured_devices")]
    public class ConfiguredDevicesConfigSegment
    {
        /// <summary>
        /// Default Devices
        /// </summary>
        [JsonProperty(PropertyName = "default_devices", Required = Required.Always)]
        public List<DefaultDevicesFieldset> DefaultDevices { get; set; }

        /// <summary>
        /// Included Devices
        /// </summary>
        [JsonProperty(PropertyName = "included_devices", Required = Required.Always)]
        public List<IncludedDevicesFieldset> IncludedDevices { get; set; }

        /// <summary>
        /// Excluded Device Types
        /// </summary>
        [JsonProperty(PropertyName = "excluded_device_types", Required = Required.Always)]
        public List<String> ExcludedDeviceTypes { get; set; }
    }
}