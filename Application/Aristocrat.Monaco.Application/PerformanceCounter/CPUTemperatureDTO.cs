namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    internal class CPUTemperatureDTO
    {
        public uint Status { get; set; }

        public uint AverageTemperature { get; set; }

        public uint MinCriticalTemperatureDistance { get; set; }

        public uint SamplesSinceLastReadFailure { get; set; }

        public uint ValidReads { get; set; }
        
        public uint InvalidReads { get; set; }
    }
}