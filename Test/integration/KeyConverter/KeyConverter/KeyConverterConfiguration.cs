namespace Aristocrat.Monaco.Test.KeyConverter
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using Kernel;

    [Serializable]
    public class KeyConverterConfiguration : ISerializable
    {
        public KeyConverterConfiguration(
            Keys onOffKey,
            IDictionary<Keys, (Type type, ICollection<object> args)> keyDownEvents,
            IDictionary<Keys, (Type type, ICollection<object> args)> keyUpEvents)
        {
            OnOffKey = onOffKey;
            KeyDownEvents = new Dictionary<Keys, (Type type, ICollection<object> args)>(keyDownEvents);
            KeyUpEvents = new Dictionary<Keys, (Type type, ICollection<object> args)>(keyUpEvents);
            PassThroughKeys = new List<Keys>();
        }

        protected KeyConverterConfiguration(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            OnOffKey = (Keys)(Keys?)info.GetValue("OnOffKey", typeof(Keys));

            var keyDownEvents = info.GetValue("KeyDownEvents", typeof(Dictionary<Keys, (Type type, ICollection<object> args)>));
            KeyDownEvents = (Dictionary<Keys, (Type type, ICollection<object> args)>)keyDownEvents;

            var keyUpEvents = info.GetValue("KeyUpEvents", typeof(Dictionary<Keys, IEvent>));
            KeyUpEvents = (Dictionary<Keys, (Type type, ICollection<object> args)>)keyUpEvents;

            try
            {
                var passThroughKeys = info.GetValue("PassThroughKeys", typeof(IList<Keys>));
                PassThroughKeys = (IList<Keys>)passThroughKeys;
            }
            catch (SerializationException)
            {
                // The serialized .bin file does not contain this member - it was generated prior to this addition.
                // Set up a default, empty list for backwards compatibility with older bin files.
                PassThroughKeys = new List<Keys>(0);
            }
        }

        public Keys OnOffKey { get; set; }

        public Dictionary<Keys, (Type type, ICollection<object> args)> KeyDownEvents { get; }

        public Dictionary<Keys, (Type type, ICollection<object> args)> KeyUpEvents { get; }

        public IList<Keys> PassThroughKeys { get; set; }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("OnOffKey", OnOffKey);
            info.AddValue("KeyDownEvents", KeyDownEvents);
            info.AddValue("KeyUpEvents", KeyUpEvents);
            info.AddValue("PassThroughKeys", PassThroughKeys);
        }
    }
}