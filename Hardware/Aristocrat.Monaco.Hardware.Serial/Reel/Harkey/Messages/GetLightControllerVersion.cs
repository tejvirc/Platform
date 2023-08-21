namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey.Messages
{
    using System;

    [Serializable]
    public class GetLightControllerVersion : HarkeySerializableMessage, ISequencedCommand
    {
        public GetLightControllerVersion()
            : base(MessageMaskType.Command, HarkeyCommandId.GetLightControllerVersion, 3, false)
        {
        }
    }
}