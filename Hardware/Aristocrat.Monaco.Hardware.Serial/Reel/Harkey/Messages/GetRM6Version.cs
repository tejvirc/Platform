namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;

    [Serializable]
    public class GetRm6Version : HarkeySerializableMessage, ISequencedCommand
    {
        public GetRm6Version()
            : base(MessageMaskType.Command, HarkeyCommandId.GetRm6Version, 3, false)
        {
        }
    }
}