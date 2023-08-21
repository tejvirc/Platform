namespace Aristocrat.Sas.Client
{
    using System;

    /// <summary>
    ///     The handler for SAS Exception   
    /// </summary>
    public interface ISasExceptionHandler : ISasExceptionAcknowledgeHandler
    {
        /// <summary>
        ///     Registers an exception processor for the give Sas Group
        /// </summary>
        /// <param name="group">The group to register the process for</param>
        /// <param name="exceptionQueue">The exception queue to process exceptions for this group</param>
        void RegisterExceptionProcessor(SasGroup group, ISasExceptionQueue exceptionQueue);

        /// <summary>
        ///     Removes an exception process for the given Sas group
        /// </summary>
        /// <param name="group">The group to remove the process from</param>
        /// <param name="exceptionQueue">The exception queue that is to be removed from listing to these events</param>
        void RemoveExceptionQueue(SasGroup group, ISasExceptionQueue exceptionQueue);

        /// <summary>
        ///     Report an exception to all applicable processors
        /// </summary>
        /// <param name="exception">The exception to post to the processors</param>
        void ReportException(ISasExceptionCollection exception);

        /// <summary>
        ///     Report an exception to all applicable processors
        /// </summary>
        /// <param name="exception">The exception to post to the processors</param>
        /// <param name="clientNumber">The client to report the exception to</param>
        void ReportException(ISasExceptionCollection exception, byte clientNumber);

        /// <summary>
        ///     Report an exception to all applicable processors
        /// </summary>
        /// <param name="exceptionProvider">The provider for the exception based on the client</param>
        /// <param name="exceptionCode">The exception code for this exception</param>
        void ReportException(Func<byte, ISasExceptionCollection> exceptionProvider, GeneralExceptionCode exceptionCode);

        /// <summary>
        ///     Remove an exception from all applicable processors
        /// </summary>
        /// <param name="exception">The exception to remove from the processors</param>
        void RemoveException(ISasExceptionCollection exception);

        /// <summary>
        ///     Remove an exception from all applicable processors
        /// </summary>
        /// <param name="exception">The exception to remove from the processors</param>
        /// <param name="clientNumber">The client number to remove the exception from</param>
        void RemoveException(ISasExceptionCollection exception, byte clientNumber);
    }
}