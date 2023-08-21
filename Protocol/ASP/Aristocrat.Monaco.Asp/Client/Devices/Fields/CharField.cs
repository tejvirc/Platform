namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using Contracts;

    public class CharField : Field
    {
        public CharField(IFieldPrototype fromField)
            : base(fromField)
        {
        }

        private char TypedValue
        {
            get => (char)Value;
            set => Value = value;
        }

        protected override void SetValue(object value)
        {
            value = Convert.ToChar(value);
            base.SetValue(value);
        }

        public override void ReadBytes(IByteArrayReader reader)
        {
            if (reader == null)
            {
                return;
            }

            TypedValue = (char)reader.ReadByte();
        }

        public override void WriteBytes(IByteArrayWriter writer)
        {
            writer?.Write((byte)TypedValue);
        }
    }
}