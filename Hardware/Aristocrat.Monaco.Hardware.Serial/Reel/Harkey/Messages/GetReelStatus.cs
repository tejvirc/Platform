namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;

    [Serializable]
    public class GetReelStatus : HarkeySerializableMessage, ISequencedCommand
    {
        public GetReelStatus()
            : base(MessageMaskType.Command, HarkeyCommandId.GetStatus, 3, false)
        {
        }
    }
}