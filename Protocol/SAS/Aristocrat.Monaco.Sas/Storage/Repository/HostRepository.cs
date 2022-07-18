namespace Aristocrat.Monaco.Sas.Storage.Repository
{
    using System.Linq;
    using Contracts.Client;
    using Contracts.Client.Configuration;
    using Contracts.SASProperties;
    using Models;
    using Protocol.Common.Storage.Repositories;

    /// <summary>
    ///     The SAS host repository helper
    /// </summary>
    public static class HostRepositoryExtensions
    {
        /// <summary>
        ///     Gets the system configuration for the SAS system
        /// </summary>
        /// <param name="repository">An instance of <see cref="IRepository{Host}"/></param>
        /// <returns>The SAS system configuration</returns>
        public static SasSystemConfiguration GetConfiguration(this IRepository<Host> repository)
        {
            return new SasSystemConfiguration
            {
                SasHostConfiguration = (from host in repository.Entities
                    from settings in repository.Context.Set<SasFeatures>().Take(1)
                    select new SasHostConfiguration
                    {
                        AccountingDenom = host.AccountingDenom,
                        ComPort = host.ComPort,
                        OverflowBehavior = settings.OverflowBehavior,
                        SasAddress = host.SasAddress
                    }).ToList(),
                SasConfiguration = (from ports in repository.Context.Set<PortAssignment>().Take(1)
                    from settings in repository.Context.Set<SasFeatures>().Take(1)
                    select new SasConfiguration
                    {
                        System = new SystemConfigurationElement
                        {
                            ControlPorts = new ControlPortsElement
                            {
                                FundTransferPort = (int)ports.FundTransferPort,
                                FundTransferType = ports.FundTransferType,
                                GameStartEndHosts = ports.GameStartEndHosts,
                                GeneralControlPort = (int)ports.GeneralControlPort,
                                LegacyBonusPort = (int)ports.LegacyBonusPort,
                                ProgressivePort = (int)ports.ProgressivePort,
                                ValidationPort = (int)ports.ValidationPort
                            },
                            ValidationType = settings.ValidationType
                        },
                        HandPay = new HandPayConfigurationElement
                        {
                            HandpayReportingType = settings.HandpayReportingType
                        }
                    }).FirstOrDefault()
            };
        }
    }
}