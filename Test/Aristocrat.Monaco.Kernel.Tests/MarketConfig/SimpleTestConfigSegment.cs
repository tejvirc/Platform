//------------------------------------------------------------------------------
// <auto-generated>
// This file was automatically generated by a tool.
// Schema Format Version: 1.0
// Generator Version: 2.3.0.0
//
// DO NOT MODIFY THIS FILE MANUALLY.
// Changes to this file may cause incorrect behavior and will be overwritten.
// </auto-generated>
//------------------------------------------------------------------------------
using Newtonsoft.Json;
using System;

namespace Aristocrat.Monaco.Kernel.MarketConfig.Models.BootExtender
{
    /// <summary>
    /// Market Jurisdiction Configuration for the Boot Extender segment
    /// </summary>
    [MarketConfigSegment("simple_test")]
    public class SimpleTestConfigSegment
    {
        /// <summary>
        /// Gaming Runnable
        /// </summary>
        [JsonProperty(PropertyName = "string", Required = Required.Always)]
        public String String { get; set; }
    }
}