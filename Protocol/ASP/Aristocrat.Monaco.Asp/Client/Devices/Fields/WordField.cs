namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using Contracts;

    public class WordField : Field
    {
        public WordField(IFieldPrototype fromField)
            : base(fromField)
        {
        }

        private ushort TypedValue
        {
            get => (ushort)Value;
            set => Value = value;
        }

        protected override void SetValue(object value)
        {
            switch (value)
            {
                case int int32:
                    value = (ushort)int32;
                    break;
                case uint uint32:
                    value = (ushort)uint32;
                    break;
                case long int64:
                    value = (ushort)int64;
                    break;
                case ulong uint64:
                    value = (ushort)uint64;
                    break;
            }

            value = Convert.ToUInt16(value);
            base.SetValue(value);
        }

        public override void ReadBytes(IByteArrayReader reader)
        {
            if (reader == null)
            {
                return;
            }

            TypedValue = reader.ReadUInt16();
        }

        public override void WriteBytes(IByteArrayWriter writer)
        {
            writer?.Write(TypedValue);
        }
    }
}