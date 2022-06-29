namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Fan;
    using Contracts.IO;
    using Kernel;

    public class FanService : IFan, IService
    {
        private readonly IIO _io;
        private readonly object _lockObject = new();
        private const int MaxPwm = 255;
        private const int MaxRpm = 4400;

        public struct TempToSpeed
        {
            public uint TemperatureLow;
            public uint TemperatureHigh;
            public uint SpeedLow;
            public uint SpeedHigh;
        };

        // temperature vs fan-speed threshold map for gen8 cabinet
        public TempToSpeed[] TempToSpeedTableGen8 = {
            new() { TemperatureLow = 80, TemperatureHigh = 100, SpeedLow = 4400, SpeedHigh = 4400 },
            new() { TemperatureLow = 75, TemperatureHigh = 80, SpeedLow = 4000, SpeedHigh = 4480 },
            new() { TemperatureLow = 65, TemperatureHigh = 75, SpeedLow = 2500, SpeedHigh = 4000 },
            new() { TemperatureLow = 35, TemperatureHigh = 65, SpeedLow = 1000, SpeedHigh = 2500 },
            new() { TemperatureLow = 0, TemperatureHigh = 35, SpeedLow = 0, SpeedHigh = 1000 }
        };

        public FanService()
            : this(ServiceManager.GetInstance().GetService<IIO>())
        {
        }

        public FanService(IIO io)
        {
            _io = io;
        }

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IFan) };

        public void Initialize()
        {
        }

        public int GetFanSpeed()
        {
            return _io.GetFanSpeed();
        }

        public int GetFanPwm()
        {
            return _io.GetFanPwm();
        }

        public int CalculateFanSpeed(float temperature)
        {
            return (int) (from m in TempToSpeedTableGen8
                where m.TemperatureLow <= temperature && m.TemperatureHigh >= temperature
                    let r = (m.SpeedHigh - m.SpeedLow) / (m.TemperatureHigh - m.TemperatureLow)
                select m.SpeedLow + Convert.ToUInt32(r * (temperature - m.TemperatureLow))).FirstOrDefault();
        }

        public int CalculatePwm(int fanSpeed)
        {
            return fanSpeed * MaxPwm / MaxRpm;
        }
    }
}