namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using Contracts;

    public class ByteField : Field
    {
        public ByteField(IFieldPrototype fromField)
            : base(fromField)
        {
        }

        private byte TypedValue
        {
            get => (byte)Value;
            set => Value = value;
        }

        protected override void SetValue(object value)
        {
            switch (value)
            {
                case short int16:
                    value = (byte)int16;
                    break;
                case ushort uint16:
                    value = (byte)uint16;
                    break;
                case int int32:
                    value = (byte)int32;
                    break;
                case uint uint32:
                    value = (byte)uint32;
                    break;
                case long int64:
                    value = (byte)int64;
                    break;
                case ulong uint64:
                    value = (byte)uint64;
                    break;
            }

            value = Convert.ToByte(value);
            base.SetValue(value);
        }

        public override void ReadBytes(IByteArrayReader reader)
        {
            if (reader == null)
            {
                return;
            }

            TypedValue = reader.ReadByte();
        }

        public override void WriteBytes(IByteArrayWriter writer)
        {
            writer?.Write(TypedValue);
        }
    }
}