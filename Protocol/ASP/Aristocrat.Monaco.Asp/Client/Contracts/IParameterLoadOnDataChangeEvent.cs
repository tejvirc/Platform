namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     An interface to load state of an object on Data Change event.
    /// </summary>
    public interface IParameterLoadOnDataChangeEvent
    {
        /// <summary>
        ///     Sets fields for the parameter given member name/value pairs. Parameters are a set of
        ///     values/properties known as members/fields that are defined in the Asp\Client\Definitions xml files.
        /// </summary>
        void SetFields(Dictionary<string, object> members);
    }
}