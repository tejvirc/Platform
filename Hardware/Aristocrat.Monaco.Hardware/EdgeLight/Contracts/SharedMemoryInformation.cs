namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    using Aristocrat.Monaco.Hardware.Contracts.EdgeLighting;

    public class SharedMemoryInformation : ISharedMemoryInformation
    {
        public string MemoryName { get; set; } = EdgeLightRuntimeParameters.EdgeLightSharedMemoryName;

        public string MutexName { get; set; } = EdgeLightRuntimeParameters.EdgeLightSharedMutexName;
    }
}