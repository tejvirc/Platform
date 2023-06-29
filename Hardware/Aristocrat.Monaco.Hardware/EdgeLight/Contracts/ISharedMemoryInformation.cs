namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    /// <summary>
    ///     ISharedMemoryInformation interface definition - used to pass parameters for accessing shared memory
    /// </summary>
    public interface ISharedMemoryInformation
    {
        /// <summary>
        ///     MemoryName - name of the shared memory
        /// </summary>
        public string MemoryName { get; }

        /// <summary>
        ///     MutexName - name of mutex to lock access
        /// </summary>
        public string MutexName { get; }
    }
}