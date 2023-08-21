namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     Interface for multi-denom aware long poll handlers.
    /// </summary>
    /// <typeparam name="TResponse">A long poll response type that must be multi-denom aware.</typeparam>
    /// <typeparam name="TData">A long poll data type that must be multi-denom aware.</typeparam>
    public interface ISasLongPollMultiDenomAwareHandler<out TResponse, in TData> : ISasLongPollHandler<TResponse, TData>
        where TResponse : LongPollMultiDenomAwareResponse
        where TData : LongPollMultiDenomAwareData
    {
    }
}