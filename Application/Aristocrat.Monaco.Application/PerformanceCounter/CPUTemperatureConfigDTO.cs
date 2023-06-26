namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    internal class CPUTemperatureConfigDTO
    {
        public uint Status { get; set; }

        public uint SamplingPeriodMS { get; set; }

        public uint SampleCount { get; set; }
    }
}