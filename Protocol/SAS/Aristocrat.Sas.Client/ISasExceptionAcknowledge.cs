namespace Aristocrat.Sas.Client
{
    using System;

    public interface ISasExceptionAcknowledgeHandler
    {
        /// <summary>
        ///     Adds a handler for a given exception code
        /// </summary>
        /// <param name="code">The exception code</param>
        /// <param name="action">The action to take when the exception has been acknowledged</param>
        void AddHandler(GeneralExceptionCode code, Action action);

        /// <summary>
        ///     Removes a handler to call when the given exception is acknowledged
        ///     as being sent.
        /// </summary>
        /// <param name="exception">The exception to remove handler</param>
        void RemoveHandler(GeneralExceptionCode exception);
    }
}