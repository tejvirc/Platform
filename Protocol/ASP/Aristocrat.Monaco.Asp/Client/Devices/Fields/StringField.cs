namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using Contracts;

    public class StringField : Field
    {
        public StringField(IFieldPrototype fromField)
            : base(fromField)
        {
        }

        private string TypedValue
        {
            get => (string)Value;
            set => Value = value;
        }

        protected override void SetValue(object value)
        {
            value = Convert.ToString(value);
            base.SetValue(value);
        }

        public override void ReadBytes(IByteArrayReader reader)
        {
            if (reader == null)
            {
                return;
            }

            TypedValue = reader.ReadString(SizeInBytes);
        }

        public override void WriteBytes(IByteArrayWriter writer)
        {
            writer?.Write(TypedValue, SizeInBytes);
        }
    }
}