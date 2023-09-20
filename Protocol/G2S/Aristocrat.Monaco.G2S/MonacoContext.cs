namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Linq;
    using Aristocrat.Monaco.Protocol.Common.Storage;
    using Common.CertificateManager.Mapping;
    using Common.Mapping;
    using Data.Mapping;
    using Data.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    /// <summary>
    ///     A Monaco specific DbContext implementation
    /// </summary>
    public sealed class MonacoContext : DbContext
    {
        private readonly string _connectionString;

        public MonacoContext(IConnectionStringResolver connectionStringResolver)
        {
            if (connectionStringResolver == null)
            {
                throw new ArgumentNullException(nameof(connectionStringResolver));
            }

            _connectionString = connectionStringResolver.Resolve();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder == null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            optionsBuilder.UseSqlite(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.ApplyConfiguration(new HostMap());
            modelBuilder.ApplyConfiguration(new ProfileDataMap());
            modelBuilder.ApplyConfiguration(new EventHandlerLogMap());
            modelBuilder.ApplyConfiguration(new EventSubscriptionMap());
            modelBuilder.ApplyConfiguration(new SupportedEventMap());
            modelBuilder.ApplyConfiguration(new MeterSubscriptionMap());

            modelBuilder.ApplyConfiguration(new ConfigChangeAuthorizeItemMap());
            modelBuilder.ApplyConfiguration(new CommHostConfigMap());
            modelBuilder.ApplyConfiguration(new CommHostConfigItemMap());
            modelBuilder.ApplyConfiguration(new CommHostConfigDeviceMap());
            modelBuilder.ApplyConfiguration(new CommChangeLogMap());

            modelBuilder.ApplyConfiguration(new OptionChangeLogMap());
            modelBuilder.ApplyConfiguration(new OptionConfigDeviceEntityMap());
            modelBuilder.ApplyConfiguration(new OptionConfigGroupMap());
            modelBuilder.ApplyConfiguration(new OptionConfigItemMap());

            modelBuilder.ApplyConfiguration(new GatVerificationRequestMap());
            modelBuilder.ApplyConfiguration(new GatComponentVerificationMap());
            modelBuilder.ApplyConfiguration(new GatSpecialFunctionMap());
            modelBuilder.ApplyConfiguration(new GatSpecialFunctionParameterMap());

            modelBuilder.ApplyConfiguration(new PkiConfigurationMap());
            modelBuilder.ApplyConfiguration(new CertificateMap());
            modelBuilder.ApplyConfiguration(new ModuleMap());
            modelBuilder.ApplyConfiguration(new PackageErrorMap());
            modelBuilder.ApplyConfiguration(new PackageMap());
            modelBuilder.ApplyConfiguration(new ScriptMap());
            modelBuilder.ApplyConfiguration(new TransferMap());

            modelBuilder.ApplyConfiguration(new PrintLogMap());
            modelBuilder.ApplyConfiguration(new VoucherDataMap());
            modelBuilder.ApplyConfiguration(new IdReaderDataMap());
            modelBuilder.ApplyConfiguration(new PackageLogMap());
            modelBuilder.ApplyConfiguration(new PendingJackpotAwardsConfiguration());

            var entityTypes = modelBuilder.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                var dateTimePropertities = entityType.GetProperties().Where(
                            x => x.PropertyInfo != null &&
                                (x.PropertyInfo.PropertyType == typeof(DateTime) || x.PropertyInfo.PropertyType == typeof(DateTime?)))
                        .ToArray();

                foreach (var property in dateTimePropertities)
                {
                    if (property.PropertyInfo.PropertyType == typeof(DateTime))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Local).ToUniversalTime()));
                    }
                    else
                    {
                        property.SetValueConverter(new ValueConverter<DateTime?, DateTime?>(v => v, v => !v.HasValue ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Local).ToUniversalTime()));
                    }
                }
            }
        }
    }
}