namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cabinet.Contracts;
    using Contracts.Cabinet;
    using Contracts.SerialPorts;
    using Kernel;
    using log4net;

    /// <summary>Implementation of Serial Ports service.</summary>
    public class SerialPortsService : ISerialPortsService, IService
    {
        private const string ComText = "COM";
        private const string Fake = "Fake";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICabinetDetectionService _cabinetIdentificationService;
        private readonly List<SerialPortInfo> _logicalSerialPorts = new();
        private readonly IList<SerialPortInfo> _serialPorts;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SerialPortsService" /> class.
        /// </summary>
        public SerialPortsService()
            : this(
                new SerialPortEnumerator(),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        public SerialPortsService(SerialPortEnumerator enumerator, ICabinetDetectionService cabinetService)
        {
            PopulateLogicalSerialPorts();

            _cabinetIdentificationService = cabinetService;
            _serialPorts = enumerator.EnumerateSerialPorts().Join(
                _logicalSerialPorts,
                x => new { x.Address, x.SerialPortType },
                y => new { y.Address, y.SerialPortType },
                (physicalPortsInfo, logicalPortInfo) => new SerialPortInfo
                {
                    Address = logicalPortInfo.Address,
                    PhysicalPortName = physicalPortsInfo.PhysicalPortName,
                    LogicalPortName = logicalPortInfo.LogicalPortName,
                    SerialPortType = logicalPortInfo.SerialPortType
                }).ToList();

            void PopulateLogicalSerialPorts()
            {
                // Actual EGMs start COM ports at an offset of 3, as designed in the FPGA.
                var offset = (_cabinetIdentificationService?.Family ?? HardwareFamily.Unknown) == HardwareFamily.Unknown  ? 0 : 3;

                for (var i = 0; i < 13; i++)
                {
                    _logicalSerialPorts.Add(
                        new SerialPortInfo
                        {
                            Address = i, LogicalPortName = $"COM{i + offset}", SerialPortType = SerialPortType.Rs232
                        });
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllLogicalPortNames()
        {
            return _serialPorts.Where(x => x.LogicalPortName != string.Empty).Select(x => x.LogicalPortName);
        }

        /// <inheritdoc />
        public void RegisterPort(string portName)
        {
            portName = portName.ToUpperInvariant();
            var serialPort = _serialPorts.FirstOrDefault(
                                 x => x.PhysicalPortName.Equals(portName, StringComparison.OrdinalIgnoreCase)) ??
                             throw new ArgumentOutOfRangeException(nameof(portName), portName);

            if (serialPort.Registered)
            {
                throw new ArgumentException($"Port {portName} is already taken");
            }

            serialPort.Registered = true;
            Logger.Debug($"Registered {portName}");
        }

        /// <inheritdoc />
        public void UnRegisterPort(string portName)
        {
            portName = portName.ToUpperInvariant();
            var serialPort = _serialPorts.FirstOrDefault(
                                 x => x.PhysicalPortName.Equals(portName, StringComparison.OrdinalIgnoreCase)) ??
                             throw new ArgumentOutOfRangeException(nameof(portName), portName);

            serialPort.Registered = false;
            Logger.Debug($"Unregistered {portName}");
        }

        /// <inheritdoc />
        public string LogicalToPhysicalName(string logicalPortName)
        {
            if (logicalPortName.Length <= ComText.Length ||
                logicalPortName.Equals(Fake, StringComparison.OrdinalIgnoreCase) ||
                _cabinetIdentificationService == null ||
                _cabinetIdentificationService.Family == HardwareFamily.Unknown)
            {
                return logicalPortName;
            }

            return _serialPorts
                .FirstOrDefault(
                    x => x.LogicalPortName.Equals(logicalPortName, StringComparison.OrdinalIgnoreCase))
                ?.PhysicalPortName ?? logicalPortName;
        }

        /// <inheritdoc />
        public string PhysicalToLogicalName(string physicalPortName)
        {
            if (physicalPortName.Length <= ComText.Length ||
                physicalPortName.Equals(Fake, StringComparison.OrdinalIgnoreCase) ||
                _cabinetIdentificationService == null ||
                _cabinetIdentificationService.Family == HardwareFamily.Unknown)
            {
                return physicalPortName;
            }

            return _serialPorts
                .FirstOrDefault(
                    x => x.PhysicalPortName.Equals(physicalPortName, StringComparison.OrdinalIgnoreCase))
                ?.LogicalPortName ?? physicalPortName;
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ISerialPortsService) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}