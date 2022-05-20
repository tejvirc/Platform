namespace Aristocrat.Monaco.Asp.Client.Devices.Fields
{
    using Contracts;

    /// <summary>
    ///     Helper class for converting field values to and from string.
    /// </summary>
    internal static class FieldValueConverter
    {
        public static string ToStringValue(this IField @this)
        {
            return @this.Value.ToString();
        }

        public static object FromString(this IFieldPrototype @this, string value)
        {
            switch (@this.Type)
            {
                case FieldType.BYTE:
                {
                    byte.TryParse(value, out var retVal);
                    return retVal;
                }
                case FieldType.WORD:
                {
                    ushort.TryParse(value, out var retVal);
                    return retVal;
                }
                case FieldType.LONG:
                {
                    int.TryParse(value, out var retVal);
                    return retVal;
                }
                case FieldType.ULONG:
                {
                    uint.TryParse(value, out var retVal);
                    return retVal;
                }
                case FieldType.FLOAT:
                {
                    float.TryParse(value, out var retVal);
                    return retVal;
                }
                case FieldType.CHAR:
                {
                    char.TryParse(value, out var retVal);
                    return retVal;
                }
            }

            return value;
        }
    }
}