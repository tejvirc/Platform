namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Interface for a data source that can get/set values of different data members.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        ///     A list of all members that this data source provides.
        /// </summary>
        IReadOnlyList<string> Members { get; }

        /// <summary>
        ///     name of the data source.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Returns the member value if exists otherwise returns null.
        /// </summary>
        /// <param name="member">Name of the data member.</param>
        /// <returns></returns>
        object GetMemberValue(string member);

        /// <summary>
        ///     Sets the value of a data member to given value. Throw's InvalidArgument exception if the value type is incorrect or
        ///     member does not exists.
        ///     This method does not fire MemberValueChanged event.
        /// </summary>
        /// <param name="member">Name of the data member to be set.</param>
        /// <param name="value">New value of the data member.</param>
        void SetMemberValue(string member, object value);

        /// <summary>
        ///     Event that is fired if the value of data member is changed by the source of the data.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "legacy code")]
        event EventHandler<Dictionary<string, object>> MemberValueChanged;
    }
}