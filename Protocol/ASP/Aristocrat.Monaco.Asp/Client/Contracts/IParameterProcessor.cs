namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System;

    /// <summary>
    ///     Interface to define asp protocol query processor.
    /// </summary>
    public interface IParameterProcessor
    {
        /// <summary>
        ///     Event that is generated when ever a parameter is changed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "legacy code")]
        event EventHandler<IParameter> ParameterEvent;

        /// <summary>
        ///     Processes the get parameter command and returns the response parameter.
        /// </summary>
        /// <param name="parameter">Parameter to get.</param>
        /// <returns>Parameter for response.</returns>
        IParameter GetParameter(IParameter parameter);

        /// <summary>
        ///     Processes the set parameter command and returns true if successful.
        /// </summary>
        /// <param name="parameter">Parameter to set.</param>
        /// <returns>Return true if successfully saves the parameter values.</returns>
        bool SetParameter(IParameter parameter);

        /// <summary>
        ///     Processes the set event command and returns true if successful. After this command any changes in given parameter
        ///     will be reported as event.
        /// </summary>
        /// <param name="parameter">Parameter to set event for.</param>
        /// <returns>Return true if successfully set the event reporting for parameter values.</returns>
        bool SetEvent(IParameter parameter);

        /// <summary>
        ///     Processes the clear event command and returns true if successful. After this command any changes in given parameter
        ///     will not be reported as event.
        /// </summary>
        /// <param name="parameter">Parameter to clear event for.</param>
        /// <returns>Return true if successfully clears the event reporting for parameter values.</returns>
        bool ClearEvent(IParameter parameter);
    }
}