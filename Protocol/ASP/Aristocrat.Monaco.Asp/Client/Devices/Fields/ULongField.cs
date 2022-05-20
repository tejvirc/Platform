namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using Contracts;

    public class ULongField : Field
    {
        public ULongField(IFieldPrototype fromField)
            : base(fromField)
        {
        }

        private uint TypedValue
        {
            get => (uint)Value;
            set => Value = value;
        }

        protected override void SetValue(object value)
        {
            switch (value)
            {
                case long int64:
                    value = (uint)int64;
                    break;
                case ulong uint64:
                    value = (uint)uint64;
                    break;
            }

            value = Convert.ToUInt32(value);
            base.SetValue(value);
        }

        public override void ReadBytes(IByteArrayReader reader)
        {
            if (reader == null)
            {
                return;
            }

            TypedValue = reader.ReadUInt32();
        }

        public override void WriteBytes(IByteArrayWriter writer)
        {
            writer?.Write(TypedValue);
        }
    }
}