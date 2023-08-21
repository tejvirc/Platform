namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public interface IReportableEventsManager
    {
        /// <summary>
        /// Retrieves a list of all events that have had event reporting enabled
        /// </summary>
        /// <returns>A list of the parameters that SetEvent has been called on</returns>
        ImmutableList<(byte @class, byte type, byte parameter)> GetAll();

        /// <summary>
        /// Records that SetEvent has been called on multiple parameters
        /// </summary>
        /// <param name="setEvents">The parameters SetEvent has been called on</param>
        void SetBatch(List<(byte @class, byte type, byte parameter)> setEvents);

        /// <summary>
        /// Records that ClearEvent has been called on multiple parameters
        /// </summary>
        /// <param name="setEvents">The parameters ClearEvent has been called on</param>
        void ClearBatch(List<(byte @class, byte type, byte parameter)> setEvents);

        /// <summary>
        /// Records that SetEvent has been called on a parameter
        /// </summary>
        /// <param name="class">The device class</param>
        /// <param name="type">The device type</param>
        /// <param name="parameter">The device parameter</param>
        void Set(byte @class, byte type, byte parameter);

        /// <summary>
        /// Records that ClearEvent has been called on a parameter
        /// </summary>
        /// <param name="class">The device class</param>
        /// <param name="type">The device type</param>
        /// <param name="parameter">The device parameter</param>
        void Clear(byte @class, byte type, byte parameter);
    }
}