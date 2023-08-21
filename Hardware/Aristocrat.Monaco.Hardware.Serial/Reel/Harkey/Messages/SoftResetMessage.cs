namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;

    [Serializable]
    public class SoftResetMessage : HarkeySerializableMessage, ISequencedCommand
    {
        public SoftResetMessage()
            : base(MessageMaskType.Command, HarkeyCommandId.SoftReset, 3, false)
        {
        }
    }
}