namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct CPUTemperature
    {
        public uint status; // must be S_OK (0) for other values to be valid, otherwise error code from GetLastError API
        public uint averageTemperature; // averaged over ALL cores and over a number of time samples
        public uint minCriticalTemperatureDistance; // averaged over a number of time samples
        public uint samplesSinceLastReadFailure; // 0 means all good, otherwise, number of time samples since read failed for all cores
        public uint validReads;
        public uint invalidReads;
    }
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct CPUTemperatureConfig
    {
        public uint status; // must be S_OK (0) for other values to be valid, otherwise error code from GetLastError API
        public uint samplingPeriodMS; // period, in milliseconds, that the driver polls core temperature sensors
        public uint sampleCount; // number of samples for calculating averages and tracking critical temperatures
    }
    public class CPUTemperatureSensor : IDisposable
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

        public CPUTemperatureSensor()
        {
            _sensorHandle = OpenCPUTemperatureSensor();
        }

        public CPUTemperature Get()
        {
            return CPUTemperatureGet(_sensorHandle);
        }
        public CPUTemperatureConfig GetConfig()
        {
            return CPUTemperatureConfigGet(_sensorHandle);
        }

        public uint SetConfig(uint sampleCount, uint samplingPeriodMS)
        {
            return CPUTemperatureConfigSet(_sensorHandle, sampleCount, samplingPeriodMS);
        }

        public void Dispose()
        {
            _sensorHandle.Dispose();
        }
    }
}