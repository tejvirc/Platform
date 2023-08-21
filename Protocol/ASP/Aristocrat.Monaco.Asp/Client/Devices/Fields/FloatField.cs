namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using Contracts;

    public class FloatField : Field
    {
        public FloatField(IFieldPrototype fromField)
            : base(fromField)
        {
        }

        private float TypedValue
        {
            get => (float)Value;
            set => Value = value;
        }

        protected override void SetValue(object value)
        {
            value = (float)Convert.ToDouble(value);
            base.SetValue(value);
        }

        public override void ReadBytes(IByteArrayReader reader)
        {
            if (reader == null)
            {
                return;
            }

            TypedValue = reader.ReadFloat();
        }

        public override void WriteBytes(IByteArrayWriter writer)
        {
            writer?.Write(TypedValue);
        }
    }
}