namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Interface that represents a field of ASP protocol.
    /// </summary>
    public interface IField : IByteSerializer, IFieldPrototype, ILoadSave
    {
        /// <summary>
        ///     The field prototype this object was created from.
        /// </summary>
        IFieldPrototype Prototype { get; }

        /// <summary>
        ///     Current value as object of the field.
        /// </summary>
        object Value { get; set; }
    }
}