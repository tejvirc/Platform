namespace Aristocrat.Monaco.Asp.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Contracts;
    using DataSources;
    using Fields;

    public class ParameterFactory : IParameterFactory
    {
        private Devices _devices;

        public ParameterFactory(ProtocolSettings protocolSettings, IDataSourceRegistry dataSourceRegistry)
        {
            DataSourceRegistry = dataSourceRegistry;
            LoadXmlDefinition(protocolSettings.DeviceDefinitionFile);
            ValidateDataMembers();
        }

        private IDataSourceRegistry DataSourceRegistry { get; }

        public IParameter Create(int deviceClass, int deviceType, int parameter)
        {
            var param = _devices[deviceClass][deviceType][parameter];
            return new Parameter(param);
        }

        public Devices DeviceDefinition { get; set; }

        public (bool DeviceClassExists, bool DeviceTypeExists, bool ParameterExists) Exists(
            int deviceClass,
            int deviceType,
            int parameter)
        {
            var cls = _devices[deviceClass];
            var type = cls?[deviceType];
            var param = type?[parameter];
            return (cls != null, type != null, param != null);
        }

        public IList<IParameterPrototype> SelectParameterPrototypes(Func<IParameterPrototype, bool> predicate)
        {
            return _devices.DeviceClasses.SelectMany(x => x.DeviceTypes.SelectMany(y => y.Parameters.Where(predicate)))
                .ToList();
        }

        public IParameter Create(INamedId deviceClass, INamedId deviceType, INamedId parameter)
        {
            return Create(deviceClass.Id, deviceType.Id, parameter.Id);
        }

        private void ValidateDataMembers()
        {
            bool IsFieldDataMembersInvalid(IFieldPrototype field)
            {
                if (field.DataSource == null)
                {
                    return false;
                }

                var dataMembers = field.Masks.Select(x => x.DataMemberName).ToList();
                if (!dataMembers.Any())
                {
                    dataMembers.Add(field.DataMemberName);
                }

                return dataMembers.Where(x => !string.IsNullOrEmpty(x)).Except(field.DataSource.Members).Any();
            }

            var fields = SelectParameterPrototypes(x => true).SelectMany(x => x.FieldsPrototype)
                .Where(IsFieldDataMembersInvalid).ToList();
            if (!fields.Any())
            {
                return;
            }

            var ex = new Exception(
                $"{fields.Count} data member names were not found in data sources. {Environment.NewLine} " +
                string.Join(Environment.NewLine, fields.Select(x => $"{x.DataSourceName} {x.DataMemberName}")));
            ex.Data.Add("FieldsNotFound", fields);
            throw ex;
        }

        private static T AddOrGet<T>(ICollection<T> list, T newElement) where T : INamedId
        {
            var element = list.FirstOrDefault(x => x.Id == newElement.Id);
            if (element != null)
            {
                return element;
            }

            list.Add(newElement);
            return newElement;
        }

        private static DeviceClass MakeMainDeviceClass(Devices devices)
        {
            var deviceClass = AddOrGet(devices.Classes, new DeviceClass { Name = "Main", Id = 1, Version = "1.0" });
            var deviceType = AddOrGet(deviceClass.Types, new DeviceType { Id = 1, Name = "Main Device" });
            deviceType.InternalParameters.Clear();
            var param = AddOrGet(
                deviceType.InternalParameters,
                CreateParameterPrototype(1, "NumberOfDevices", deviceClass, deviceType));
            param.FieldsPrototypeInternal.Clear();
            AddOrGet(
                param.FieldsPrototypeInternal,
                new FieldPrototype
                {
                    Id = 0,
                    Name = "NumberOfDevices",
                    SizeInBytes = 1,
                    DefaultValue = devices.DeviceClasses.SelectMany(x => x.DeviceTypes)
                        .Count().ToString()
                });
            MakeAggregatedParameterPrototype(deviceClass, deviceType);
            return deviceClass;
        }

        private static void AddTypeToMainDeviceClass(
            DeviceType mainDeviceType,
            DeviceClass newDeviceClass,
            INamedId newDeviceType)
        {
            var param = AddOrGet(
                mainDeviceType.InternalParameters,
                CreateParameterPrototype(
                    mainDeviceType.InternalParameters.Count,
                    "Device" + newDeviceType.Id,
                    newDeviceClass,
                    mainDeviceType));
            param.FieldsPrototypeInternal.Clear();
            AddOrGet(
                param.FieldsPrototypeInternal,
                new FieldPrototype
                {
                    Id = 0, Name = "Class", SizeInBytes = 1, DefaultValue = newDeviceClass.Id.ToString()
                });
            AddOrGet(
                param.FieldsPrototypeInternal,
                new FieldPrototype
                {
                    Id = 1, Name = "Type", SizeInBytes = 1, DefaultValue = newDeviceType.Id.ToString()
                });
        }

        private static void MakeAggregatedParameterPrototype(DeviceClass deviceClass, DeviceType deviceType)
        {
            var prototype = AddOrGet(
                deviceType.InternalParameters,
                CreateParameterPrototype(0, deviceType.Name, deviceClass, deviceType));
            deviceType.InternalParameters.Sort((x, y) => x.Id.CompareTo(y.Id));
            prototype.FieldsPrototypeInternal = deviceType.InternalParameters.Where(x => x.Id != 0)
                .SelectMany(x => x.FieldsPrototypeInternal).ToList();
        }

        private static ParameterPrototype CreateParameterPrototype(
            int id,
            string name,
            DeviceClass deviceClass,
            INamedId deviceType)
        {
            return new ParameterPrototype
            {
                Id = id,
                Name = name,
                ClassId = deviceClass,
                TypeId = deviceType,
                EgmAccessType = AccessType.ReadOnly,
                MciAccessType = AccessType.ReadOnly,
                EventAccessType = EventAccessType.Never
            };
        }

        private void SetFieldDataSource(IParameterPrototype paramImpl)
        {
            var fields = paramImpl.FieldsPrototype;
            foreach (var f in fields)
            {
                if (string.IsNullOrEmpty(f.DataSourceName) || string.IsNullOrEmpty(f.DataMemberName))
                {
                    continue;
                }

                f.DataSource = DataSourceRegistry?.GetDataSource(f.DataSourceName);
                if (f.DataSource is DummyDataSource)
                {
                    f.DataSource?.SetMemberValue(f.DataMemberName, f.FromString(f.DefaultValue));
                }
            }
        }

        private void LoadXmlDefinition(Stream stream)
        {
            var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(Devices))
                .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
            var xmlSerializer = new XmlSerializer(typeof(Devices), theXmlRootAttribute ?? new XmlRootAttribute(nameof(Devices)));

            _devices = (Devices)xmlSerializer.Deserialize(stream);
            DeviceDefinition = _devices;
            var mainDeviceClass = MakeMainDeviceClass(_devices);
            var mainDeviceType = mainDeviceClass.Types.First();
            foreach (var deviceClass in _devices.DeviceClasses)
            {
                foreach (var deviceType in deviceClass.DeviceTypes)
                {
                    foreach (var param in deviceType.Parameters)
                    {
                        if (!(param is ParameterPrototype paramImpl))
                        {
                            continue;
                        }

                        paramImpl.ClassId = deviceClass;
                        paramImpl.TypeId = deviceType;
                        SetFieldDataSource(paramImpl);
                    }

                    MakeAggregatedParameterPrototype(deviceClass, deviceType);
                    AddTypeToMainDeviceClass(mainDeviceType, deviceClass, deviceType);
                }
            }

            MakeAggregatedParameterPrototype(mainDeviceClass, mainDeviceType);
        }

        private void LoadXmlDefinition(string fileOrResource)
        {
            Stream stream;
            if (File.Exists(fileOrResource))
            {
                stream = new FileStream(fileOrResource, FileMode.Open, FileAccess.Read);
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                stream = assembly.GetManifestResourceStream(fileOrResource);
            }

            using (stream)
            {
                LoadXmlDefinition(stream);
            }
        }
    }
}