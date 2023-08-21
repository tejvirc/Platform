namespace Aristocrat.Monaco.Gaming.Configuration
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    [ProtoContract]
    internal class RestrictionMap
    {
        [ProtoMember(1)]
        public string ThemeId { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
    }
}