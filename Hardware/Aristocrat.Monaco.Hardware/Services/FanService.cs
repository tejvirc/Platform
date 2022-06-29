namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Contracts.Fan;
    using Contracts.IO;
    using Kernel;
    using OpenHardwareMonitor.Hardware;

    public class FanService : IFan, IService, IDisposable
    {
        private readonly IIO _io;
        private readonly object _lockObject = new();
        private const int MaxPwm = 255;
        private const int MaxRpm = 4400;


        // lock
        private object mLock = new object();

        // update timer
        private System.Timers.Timer mUpdateTimer = null;

        //public event UpdateTimerEventHandler onUpdateCallback;
        public delegate void UpdateTimerEventHandler();

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

        private int currentPwm = 0;

        public void Initialize()
        {
            currentPwm=GetFanPwm();

            startUpdate();
        }

        public void startUpdate()
        {
            Monitor.Enter(mLock);

            mUpdateTimer = new System.Timers.Timer();
            mUpdateTimer.Interval = 1000;
            mUpdateTimer.Elapsed += onUpdateTimer;
            mUpdateTimer.Start();

            Monitor.Exit(mLock);
        }

        private void onUpdateTimer(object sender, EventArgs e)
        {
            var temperature = GetCpuTemperature();
            var fanSpeed = CalculateFanSpeed(temperature);
            var fanPwm = CalculatePwm(fanSpeed);

            if (currentPwm != fanPwm)
            {
                if(SetFanPwm(fanPwm))
                    currentPwm=fanPwm;
            }
        }

        public int GetFanSpeed()
        {
            return _io.GetFanSpeed();
        }

        public int GetFanPwm()
        {
            return _io.GetFanPwm();
        }

        public bool SetFanPwm(int pwm)
        {
            return _io.SetFanPwm(pwm);
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

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        public static float GetCpuTemperature()
        {
            float result = 0;

            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.Accept(updateVisitor);
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            if (computer.Hardware[i].Sensors[j].Value > result)
                                result = computer.Hardware[i].Sensors[j].Value.Value;
                        }
                    }
                }
            }
            computer.Close();

            return result;
        }

        protected virtual void Dispose(bool isNative)
        {
            mUpdateTimer.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}