namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Concurrent;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.SQLite;
    using System.Linq;
    using System.Reflection;
    using Common.CertificateManager.Mapping;
    using Common.Mapping;
    using Data.Mapping;
    using Monaco.Common.Storage;
    using SQLite.CodeFirst;

    /// <summary>
    ///     A Monaco specific DbContext implementation
    /// </summary>
    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class MonacoContext : DbContext
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ObjectCache =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="MonacoContext" /> class.
        /// </summary>
        public MonacoContext()
            : base("name=MonacoContext")
        {
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized +=
                (sender, e) => FixDateTimeValues(e.Entity);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MonacoContext" /> class.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        public MonacoContext(string connectionString)
            : base(new SQLiteConnection(connectionString), true)
        {
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized +=
                (sender, e) => FixDateTimeValues(e.Entity);
        }

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // NOTE:  If you're adding a new table make sure it's included in the clean-up that occurs in StorageHandler.cs (order is important)
            modelBuilder.Configurations.Add(new HostMap());
            modelBuilder.Configurations.Add(new ProfileDataMap());

            modelBuilder.Configurations.Add(new EventHandlerLogMap());
            modelBuilder.Configurations.Add(new EventSubscriptionMap());
            modelBuilder.Configurations.Add(new SupportedEventMap());
            modelBuilder.Configurations.Add(new MeterSubscriptionMap());

            modelBuilder.Configurations.Add(new ConfigChangeAuthorizeItemMap());
            modelBuilder.Configurations.Add(new CommHostConfigMap());
            modelBuilder.Configurations.Add(new CommHostConfigItemMap());
            modelBuilder.Configurations.Add(new CommHostConfigDeviceMap());
            modelBuilder.Configurations.Add(new CommChangeLogMap());

            modelBuilder.Configurations.Add(new OptionChangeLogMap());
            modelBuilder.Configurations.Add(new OptionConfigDeviceEntityMap());
            modelBuilder.Configurations.Add(new OptionConfigGroupMap());
            modelBuilder.Configurations.Add(new OptionConfigItemMap());

            modelBuilder.Configurations.Add(new GatVerificationRequestMap());
            modelBuilder.Configurations.Add(new GatComponentVerificationMap());
            modelBuilder.Configurations.Add(new GatSpecialFunctionMap());
            modelBuilder.Configurations.Add(new GatSpecialFunctionParameterMap());

            modelBuilder.Configurations.Add(new PkiConfigurationMap());
            modelBuilder.Configurations.Add(new CertificateMap());

            modelBuilder.Configurations.Add(new ModuleMap());
            modelBuilder.Configurations.Add(new PackageErrorMap());
            modelBuilder.Configurations.Add(new PackageMap());
            modelBuilder.Configurations.Add(new ScriptMap());
            modelBuilder.Configurations.Add(new TransferMap());

            modelBuilder.Configurations.Add(new PrintLogMap());
            modelBuilder.Configurations.Add(new VoucherDataMap());
            modelBuilder.Configurations.Add(new IdReaderDataMap());
            modelBuilder.Configurations.Add(new PackageLogMap());

            Database.SetInitializer(new SqliteCreateDatabaseIfNotExists<MonacoContext>(modelBuilder, true));
        }

        private static void FixDateTimeValues(object entity)
        {
            if (entity == null)
            {
                return;
            }

            var type = entity.GetType();
            var properties = ObjectCache.GetOrAdd(
                type,
                t =>
                {
                    return t.GetProperties().Where(
                            x => x.PropertyType == typeof(DateTime) || x.PropertyType == typeof(DateTime?))
                        .ToArray();
                });

            foreach (var property in properties)
            {
                var dt = property.PropertyType == typeof(DateTime?)
                    ? (DateTime?)property.GetValue(entity)
                    : (DateTime)property.GetValue(entity);

                if (dt == null)
                {
                    continue;
                }

                property.SetValue(entity, DateTime.SpecifyKind(dt.Value, DateTimeKind.Local).ToUniversalTime());
            }
        }
    }
}