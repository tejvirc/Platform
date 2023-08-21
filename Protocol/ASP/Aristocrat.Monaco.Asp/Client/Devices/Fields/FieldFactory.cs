namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using System;
    using Contracts;

    public static class FieldFactory
    {
        public static IField CreateField(IFieldPrototype fromField)
        {
            switch (fromField.Type)
            {
                case FieldType.BYTE:
                    return new ByteField(fromField);
                case FieldType.CHAR:
                    return new CharField(fromField);
                case FieldType.FLOAT:
                    return new FloatField(fromField);
                case FieldType.INT:
                case FieldType.LONG:
                    return new LongField(fromField);
                case FieldType.ULONG:
                    return new ULongField(fromField);
                case FieldType.WORD:
                    return new WordField(fromField);
                case FieldType.STRING:
                    return new StringField(fromField);
                case FieldType.BCD:
                    break;
                case FieldType.UNKNOWN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}