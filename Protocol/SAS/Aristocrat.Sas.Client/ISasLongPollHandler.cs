namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;

    public interface ISasLongPollHandler
    {
        /// <summary>
        /// Gets the SAS long poll commands this handler handles
        /// </summary>
        /// <returns>The SAS commands this handler handles</returns>
        List<LongPoll> Commands { get; }
    }

    public interface ISasLongPollHandler<out TResponse, in TData> : ISasLongPollHandler where TResponse : LongPollResponse where TData : LongPollData 
    {
        /// <summary>
        /// Handler for the long poll
        /// </summary>
        /// <param name="data">
        /// The data needed to handle the command
        /// </param>
        /// <returns>Any response required by the long poll or null if no response required</returns>
        TResponse Handle(TData data);
    }
}
