namespace Aristocrat.Monaco.Bingo.Services.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using Common.Storage.Model;
    using log4net;
    using Protocol.Common.Storage.Entity;

    public class CertificateService : ICertificateService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateService"/> class.
        /// </summary>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory"/>.</param>
        public CertificateService(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(ICertificateService) };

        public void Initialize()
        {
            var certificates = _unitOfWorkFactory.Invoke(x => x.Repository<Certificate>().Queryable().ToList());
            foreach (var certificate in certificates)
            {
                AddIfNotExist(
                    StoreName.Root,
                    StoreLocation.LocalMachine,
                    X509FindType.FindByThumbprint,
                    certificate.Thumbprint,
                    () => new X509Certificate2(certificate.RawData));
            }
        }

        public IEnumerable<X509Certificate2> GetCertificates() =>
            GetCertificates(StoreName.Root, StoreLocation.LocalMachine, X509FindType.FindByThumbprint);

        private IEnumerable<X509Certificate2> GetCertificates(
            StoreName name,
            StoreLocation location,
            X509FindType findType)
        {
            var certificates = _unitOfWorkFactory.Invoke(x => x.Repository<Certificate>().Queryable().ToList());
            using var store = new X509Store(name, location);
            store.Open(OpenFlags.ReadOnly);
            foreach (var certificate in certificates)
            {
                var x509Certificate2 = store.Certificates.Find(findType, certificate.Thumbprint, false)
                    .Cast<X509Certificate2>()
                    .SingleOrDefault();
                if (x509Certificate2 is null)
                {
                    continue;
                }

                yield return x509Certificate2;
            }
        }

        private void AddIfNotExist(
            StoreName name,
            StoreLocation location,
            X509FindType findType,
            object findValue,
            Func<X509Certificate2> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            using var store = new X509Store(name, location);
            store.Open(OpenFlags.ReadWrite);
            var certificates = store.Certificates.Find(findType, findValue, false)
                .OfType<X509Certificate2>();
            if (certificates.Any())
            {
                return;
            }

            var certificate = factory();
            Logger.Debug($"Installing certificate with thumbprint {certificate.Thumbprint}.");
            store.Add(certificate);
        }
    }
}
