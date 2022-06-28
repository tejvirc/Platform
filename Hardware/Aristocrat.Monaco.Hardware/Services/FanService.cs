namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Hardware.Contracts.Fan;
    using Contracts.Battery;
    using Contracts.IO;
    using Kernel;

    public class FanService : IFan, IService
    {
        private readonly IIO _io;
        private readonly object _lockObject = new object();

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
    }
}