namespace Aristocrat.Monaco.Mgam.Services.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Aristocrat.Mgam.Client.Logging;
    using Common;
    using Common.Data.Models;
    using Kernel;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Manages installation and removal of certificate files.
    /// </summary>
    public class CertificateService : ICertificateService, IService
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateService"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="unitOfWorkFactory"><see cref="IUnitOfWorkFactory"/>.</param>
        public CertificateService(
            ILogger<CertificateService> logger,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _logger = logger;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(ICertificateService) };

        public void Initialize()
        {
            Certificate certificate;

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                certificate = unitOfWork.Repository<Certificate>().Queryable().Single();
            }

            AddIfNotExist(
                StoreName.AuthRoot,
                StoreLocation.LocalMachine,
                X509FindType.FindByThumbprint,
                certificate.Thumbprint,
                () => new X509Certificate2(certificate.RawData));
        }

        public void ReplaceCaCertificate(string path)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var certificates = unitOfWork.Repository<Certificate>();

                var certificate = certificates.Queryable().SingleOrDefault();
                if (certificate != null)
                {
                    Delete(
                        StoreName.AuthRoot,
                        StoreLocation.LocalMachine,
                        X509FindType.FindByThumbprint,
                        certificate.Thumbprint);

                    certificates.Delete(certificate);
                }

                certificate = CertificateExtensions.LoadCertificate(path);

                certificates.Add(certificate);

                AddIfNotExist(
                    StoreName.AuthRoot,
                    StoreLocation.LocalMachine,
                    X509FindType.FindByThumbprint,
                    certificate.Thumbprint,
                    () => new X509Certificate2(certificate.RawData));

                unitOfWork.SaveChanges();
            }
        }

        private static bool Exists(StoreName name, StoreLocation location, X509FindType findType, object findValue) =>
            Get(name, location, findType, findValue) != null;

        private static void Delete(StoreName name, StoreLocation location, X509FindType findType, object findValue)
        {
            var certificate = Get(name, location, findType, findValue);
            if (certificate == null)
            {
                return;
            }

            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Remove(certificate);
            }
        }

        private static X509Certificate2 Get(
            StoreName name,
            StoreLocation location,
            X509FindType findType,
            object findValue)
        {
            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadOnly);

                return store.Certificates.Find(findType, findValue, false)
                    .Cast<X509Certificate2>()
                    .SingleOrDefault();
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

            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadWrite);

                var certificate = store.Certificates.Find(findType, findValue, false)
                    .OfType<X509Certificate2>()
                    .FirstOrDefault();
                if (certificate == null)
                {
                    certificate = factory();
                    _logger.LogDebug($"Installing certificate with thumbprint {certificate.Thumbprint}.");
                    store.Add(certificate);
                }
            }
        }
    }
}
