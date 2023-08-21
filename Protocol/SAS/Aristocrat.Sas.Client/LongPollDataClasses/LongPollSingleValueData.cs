namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <inheritdoc />
    public class LongPollSingleValueData<T> : LongPollData
    {
        public LongPollSingleValueData(T value)
        {
            Value = value;
        }

        public LongPollSingleValueData()
        {
        }

        public T Value { get; set; }
    }
}