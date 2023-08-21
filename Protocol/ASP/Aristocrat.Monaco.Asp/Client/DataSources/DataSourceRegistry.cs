namespace Aristocrat.Monaco.Asp.Client.DataSources
{ 
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using System.Linq;

    public class DataSourceRegistry : IDataSourceRegistry
    {
        private readonly Dictionary<string, IDataSource> _dataSourceDictionary = new Dictionary<string, IDataSource>();

        public DataSourceRegistry(IEnumerable<IDataSource> dataSources)
        {
            foreach (var dataSource in dataSources)
            {
                RegisterDataSource(dataSource);
            }
        }

        public IDataSource GetDataSource(string name)
        {
            if (!_dataSourceDictionary.ContainsKey(name))
            {
                _dataSourceDictionary.Add(name, new DummyDataSource { Name = name });
            }

            return _dataSourceDictionary[name];
        }

        public void RegisterDataSource(IDataSource dataSource)
        {
            _dataSourceDictionary.Add(dataSource.Name, dataSource);

            if (dataSource.GetType().GetProperties().
                Where(o => o.GetCustomAttribute(typeof(DatasourceRegistryAttribute)) is DatasourceRegistryAttribute
                && o.PropertyType == typeof(IDataSourceRegistry)).FirstOrDefault() is PropertyInfo propertyInfo)
            {
                propertyInfo.SetValue(dataSource, this);
            }
        }
    }
}