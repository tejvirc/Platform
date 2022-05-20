namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    using Hardware.Contracts.EdgeLighting;

    internal interface IEdgeLightRenderer : IEdgeLightToken
    {
        void Setup(IEdgeLightManager edgeLightManager);
        void Update();
        void Clear();
    }
}