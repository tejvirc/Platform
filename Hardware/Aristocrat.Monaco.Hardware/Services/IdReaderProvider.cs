namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts.IdReader;
    using Contracts.SharedDevice;
    using IdReader;
    using Kernel;
    using log4net;

    /// <summary>An ID reader provider.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.SharedDevice.DeviceProvider{Aristocrat.Monaco.Hardware.Contracts.IdReader.IIdReader}" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.IdReader.IIdReaderProvider" />
    /// <seealso cref="T:Aristocrat.Monaco.Kernel.IPropertyProvider" />
    public class IdReaderProvider : DeviceProvider<IIdReader>, IIdReaderProvider, IPropertyProvider
    {
        public const string AllReaders = "IdReaderProvider.AllReaders";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        
        private int _nextId;

        /// <inheritdoc />
        public override string Name => typeof(IdReaderProvider).ToString();

        /// <inheritdoc />
        public override ICollection<Type> ServiceTypes => new[] { typeof(IIdReaderProvider) };

        /// <inheritdoc />
        public override IIdReader CreateAdapter(string name)
        {
            var adapter = new IdReaderAdapter { IdReaderId = Interlocked.Increment(ref _nextId) };

            return InsertAdapter(adapter, adapter.IdReaderId) ? adapter : null;
        }

        /// <inheritdoc />
        public void SetIdValidation(int idReaderId, Identity identity)
        {
            foreach (var adapter in Adapters.Where(a => a.IdReaderId != idReaderId))
            {
                if (adapter is IdReaderAdapter item)
                {
                    item.SetIdValidation(null);
                }
            }

            var reader = this[idReaderId] as IdReaderAdapter;

            reader?.SetIdValidation(identity);
        }

        /// <inheritdoc />
        public void SetValidationComplete(int idReaderId)
        {
            var reader = this[idReaderId] as IdReaderAdapter;
            reader?.SetValidationComplete();
        }

        /// <inheritdoc />
        public void SetValidationFailed(int idReaderId)
        {
            var reader = this[idReaderId] as IdReaderAdapter;
            reader?.SetValidationFailed();
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection => new Dictionary<string, object>
        {
            { AllReaders, Adapters }
        };

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case AllReaders:
                    return Adapters;
                default:
                    var error = $"Unknown property found: {propertyName}";
                    Logger.Fatal(error);
                    throw new UnknownPropertyException(error);
            }
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            // No external sets for this provider...
        }
    }
}
