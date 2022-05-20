namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Interface to represent a parameter of asp protocol.
    /// </summary>
    public interface IParameter : IParameterPrototype, IByteSerializer, ILoadSave, IParameterLoadOnDataChangeEvent
    {
        /// <summary>
        ///     Prototype for this parameter.
        /// </summary>
        IParameterPrototype Prototype { get; }

        /// <summary>
        ///     Fields in this parameter.
        /// </summary>
        IReadOnlyList<IField> Fields { get; }
    }
}