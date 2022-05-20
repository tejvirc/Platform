namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Interface to represent attributes of a asp protocol parameter filed.
    /// </summary>
    public interface IFieldPrototype : INamedId, IDataBindable
    {
        /// <summary>
        ///     Default value of the field.
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        ///     Type of the field.
        /// </summary>
        FieldType Type { get; }

        /// <summary>
        ///     Size of the field in bytes.
        /// </summary>
        int SizeInBytes { get; }

        IReadOnlyList<IMask> Masks { get; }
    }
}