namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    using System;

    public class AcceptsAttribute : Attribute
    {
        public AcceptsAttribute(params int[] types)
        {
            Types = types;
        }

        public int[] Types { get; set; }
    }
}