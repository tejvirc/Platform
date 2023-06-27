namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CPUTemperature
    {
        public uint status; // must be S_OK (0) for other values to be valid, otherwise error code from GetLastError API
        public uint averageTemperature; // averaged over ALL cores and over a number of time samples
        public uint minCriticalTemperatureDistance; // averaged over a number of time samples
        public uint samplesSinceLastReadFailure; // 0 means all good, otherwise, number of time samples since read failed for all cores
        public uint validReads;
        public uint invalidReads;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CPUTemperatureConfig
    {
        public uint status; // must be S_OK (0) for other values to be valid, otherwise error code from GetLastError API
        public uint samplingPeriodMS; // period, in milliseconds, that the driver polls core temperature sensors
        public uint sampleCount; // number of samples for calculating averages and tracking critical temperatures
    }

    internal sealed class CPUTemperatureSensor : IDisposable
    {
        [DllImport(@"CPUTemperatureSensor.dll")]
        private static extern SafeFileHandle OpenCPUTemperatureSensor();

        [DllImport(@"CPUTemperatureSensor.dll")]
        private static extern CPUTemperature CPUTemperatureGet(SafeFileHandle sensorDevice);
        [DllImport(@"CPUTemperatureSensor.dll")]
        private static extern CPUTemperatureConfig CPUTemperatureConfigGet(SafeFileHandle sensorDevice);

        [DllImport(@"CPUTemperatureSensor.dll")]
        private static extern uint CPUTemperatureConfigSet(SafeFileHandle sensorDevice, uint sampleCount, uint samplingPeriodMS);

        private readonly SafeFileHandle _sensorHandle; // greatly simplifies implementation of IDisposable pattern

        internal CPUTemperatureSensor()
        {
            try
            {
                _sensorHandle = OpenCPUTemperatureSensor();
            }
            catch(DllNotFoundException)
            {
                _sensorHandle = null;
            }
        }

        internal CPUTemperatureDTO Get()
        {
            if (_sensorHandle == null)
            {
                return new CPUTemperatureDTO();
            }

            var rawResult = CPUTemperatureGet(_sensorHandle);

            var result = new CPUTemperatureDTO
            {
                Status = rawResult.status,
                AverageTemperature = rawResult.averageTemperature,
                MinCriticalTemperatureDistance = rawResult.minCriticalTemperatureDistance,
                SamplesSinceLastReadFailure = rawResult.samplesSinceLastReadFailure,
                ValidReads = rawResult.validReads,
                InvalidReads = rawResult.invalidReads
            };

            return result;
        }

        internal CPUTemperatureConfigDTO GetConfig()
        {
            if (_sensorHandle == null)
            {
                return new CPUTemperatureConfigDTO();
            }

            var rawResult = CPUTemperatureConfigGet(_sensorHandle);

            var result = new CPUTemperatureConfigDTO
            {
                Status = rawResult.status,
                SamplingPeriodMS = rawResult.samplingPeriodMS,
                SampleCount = rawResult.sampleCount
            };

            return result;
        }

        internal uint SetConfig(uint sampleCount, uint samplingPeriodMS)
        {
            if (_sensorHandle == null)
            {
                return 0;
            }

            return CPUTemperatureConfigSet(_sensorHandle, sampleCount, samplingPeriodMS);
        }

        public void Dispose()
        {
            if (_sensorHandle == null)
            {
                return;
            }
            
            _sensorHandle.Dispose();
        }
    }
}