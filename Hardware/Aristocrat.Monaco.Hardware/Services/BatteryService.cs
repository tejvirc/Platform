namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using Contracts.Battery;
    using Contracts.IO;
    using Kernel;

    public class BatteryService : IBattery, IService
    {
        private readonly IIO _io;
        private readonly object _lockObject = new object();

        public BatteryService()
            : this(ServiceManager.GetInstance().GetService<IIO>())
        {
        }

        public BatteryService(IIO io)
        {
            _io = io;
        }

        public (bool Battery1Result, bool Battery2Result) Test()
        {
            lock (_lockObject)
            {
                if (_io.LogicalState == IOLogicalState.Disabled)
                {
                    return (true, true);
                }

                return (_io.TestBattery(0), _io.TestBattery(1));
            }
        }

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IBattery) };

        public void Initialize()
        {
        }
    }
}