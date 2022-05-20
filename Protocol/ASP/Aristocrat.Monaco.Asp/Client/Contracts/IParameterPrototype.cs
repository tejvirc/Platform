namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Interface that defines the attributes of a parameter of asp protocol.
    /// </summary>
    public interface IParameterPrototype : INamedId
    {
        /// <summary>
        ///     Device class id of this parameter.
        /// </summary>
        INamedId ClassId { get; }

        /// <summary>
        ///     Device type id of this parameter.
        /// </summary>
        INamedId TypeId { get; }

        /// <summary>
        ///     Field prototypes contained in this parameter.
        /// </summary>
        IReadOnlyList<IFieldPrototype> FieldsPrototype { get; }

        /// <summary>
        ///     Access type for gaming device for this parameter.
        /// </summary>
        AccessType EgmAccessType { get; }

        /// <summary>
        ///     Access type for event reporting for this parameter.
        /// </summary>
        EventAccessType EventAccessType { get; }

        /// <summary>
        ///     Access type for host device for this parameter.
        /// </summary>
        AccessType MciAccessType { get; }

        /// <summary>
        ///     Size in bytes of this parameter in bytes.
        /// </summary>
        int SizeInBytes { get; }
    }
}