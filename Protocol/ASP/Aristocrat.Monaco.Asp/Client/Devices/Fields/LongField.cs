namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using Contracts;

    public class LongField : Field
    {
        public LongField(IFieldPrototype fromField)
            : base(fromField)
        {
        }

        private int TypedValue
        {
            get => (int)Value;
            set => Value = value;
        }

        protected override void SetValue(object value)
        {
            switch (value)
            {
                case long int64:
                    value = (int)int64;
                    break;
                case ulong uint64:
                    value = (int)uint64;
                    break;
            }

            value = Convert.ToInt32(value);
            base.SetValue(value);
        }

        public override void ReadBytes(IByteArrayReader reader)
        {
            if (reader == null)
            {
                return;
            }

            TypedValue = reader.ReadInt32();
        }

        public override void WriteBytes(IByteArrayWriter writer)
        {
            writer?.Write(TypedValue);
        }
    }
}