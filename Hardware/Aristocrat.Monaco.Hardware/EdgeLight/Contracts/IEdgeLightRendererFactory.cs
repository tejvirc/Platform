namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    /// <summary>
    ///     IEdgeLightRendererFactory interface definition
    /// </summary>
    internal interface IEdgeLightRendererFactory
    {
        /// <summary>
        ///     Method to create a renderer based on the supplied parameter type
        /// </summary>
        /// <typeparam name="TParametersType"></typeparam>
        /// <param name="parameters">The PatternParameters type used to determine edge light behavior</param>
        /// <returns>IEdgeLightRenderer</returns>
        IEdgeLightRenderer CreateRenderer<TParametersType>(TParametersType parameters);
    }
}