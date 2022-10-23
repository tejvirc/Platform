namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using System.Reflection;
    using Data.Model;
    using Data.CommConfig;
    using Aristocrat.Monaco.G2S.Data.OptionConfig;
    using Aristocrat.Monaco.G2S.Common.GAT.Storage;
    using Aristocrat.Monaco.G2S.Common.CertificateManager.Models;
    using Aristocrat.Monaco.G2S.Common.PackageManager.Storage;
    using Aristocrat.Monaco.G2S.Common.CertificateManager.Mapping;
    using Aristocrat.Monaco.G2S.Common.Mapping;
    using Aristocrat.Monaco.G2S.Data.Mapping;

    /// <summary>
    ///     A Monaco specific DbContext implementation
    /// </summary>
    public class MonacoContext : DbContext
    {
        private readonly string _connectionString;

        public MonacoContext(string connectionString)
        {
            _connectionString = connectionString;
            Database.EnsureCreated();
        }

        public DbSet<Data.Model.Host> Host { get; set; }
        public DbSet<ProfileData> ProfileData { get; set; }
        public DbSet<EventHandlerLog> EventHandlerLog { get; set; }
        public DbSet<EventSubscription> EventSubscription { get; set; }
        public DbSet<SupportedEvent> SupportedEvent { get; set; }
        public DbSet<MeterSubscription> MeterSubscription { get; set; } 
        public DbSet<CommHostConfig> CommHostConfig { get; set; }
        public DbSet<CommHostConfigItem> CommHostConfigItem { get; set; }
        public DbSet<CommHostConfigDevice> CommHostConfigDevice { get; set; }
        public DbSet<CommChangeLog> CommChangeLog { get; set; }
        public DbSet<ConfigChangeAuthorizeItem> ConfigChangeAuthorizeItem { get; set; }
        public DbSet<OptionChangeLog> OptionChangeLog { get; set; }
        public DbSet<OptionConfigDeviceEntity> OptionConfigDevice { get; set; }
        public DbSet<OptionConfigGroup> OptionConfigGroup { get; set; }
        public DbSet<OptionConfigItem> OptionConfigItem { get; set; }
        public DbSet<GatVerificationRequest> GatVerificationRequest { get; set; }
        public DbSet<GatComponentVerification> GatComponentVerification { get; set; }
        public DbSet<GatSpecialFunction> GatSpecialFunction { get; set; }
        public DbSet<GatSpecialFunctionParameter> GatSpecialFunctionParameter { get; set; }
        public DbSet<PkiConfiguration> PkiConfiguration { get; set; }
        public DbSet<Certificate> Certificate { get; set; }
        public DbSet<Common.PackageManager.Storage.Module> Module { get; set; }
        public DbSet<PackageError> PackageError { get; set; }
        public DbSet<Package> Package { get; set; }
        public DbSet<Script> Script { get; set; }
        public DbSet<TransferEntity> Transfer { get; set; }
        public DbSet<PrintLog> PrinterLog { get; set; }
        public DbSet<VoucherData> VoucherData { get; set; }
        public DbSet<IdReaderData> IdReaderData { get; set; }
        public DbSet<PackageLog> PackageLog { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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