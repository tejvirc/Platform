namespace Aristocrat.Monaco.G2S.Common.Tests.DHCP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.DHCP;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VendorSpecificInformationTest
    {
        [TestMethod]
        public void WhenParseVendorSpecificInformationEmptyValueExpectDefaults()
        {
            var value = string.Empty;
            var info = VendorSpecificInformation.Create(value);

            var certificateManagerDefinitions = info.CertificateManagerDefinitions;
            var certificateStatusDefinitions = info.CertificateStatusDefinitions;
            var commConfigDefinitions = info.CommConfigDefinitions;

            Assert.AreEqual(1, certificateManagerDefinitions.Count);
            Assert.AreEqual(1, certificateStatusDefinitions.Count);
            Assert.AreEqual(1, commConfigDefinitions.Count);

            var ocspAcceptPrevGoodPeriodMin = info.OcspAcceptPrevGoodPeriodMin;
            var ocspMinimumPeriodForOfflineMin = info.OcspMinimumPeriodForOfflineMin;
            var ocspReauthPeriodMin = info.OcspReauthPeriodMin;

            Assert.AreEqual(600, ocspReauthPeriodMin);
            Assert.AreEqual(240, ocspMinimumPeriodForOfflineMin);
            Assert.AreEqual(720, ocspAcceptPrevGoodPeriodMin);

            var certificateManagerDefinition = certificateManagerDefinitions.First();
            var certificateStatusDefinition = certificateStatusDefinitions.First();
            var commConfigDefinition = commConfigDefinitions.First();

            AssertServiceDefinition(
                certificateManagerDefinition,
                new ServiceDefinition(
                    "gsaCM",
                    new Uri("http://gsaCM.gsa.com:80"),
                    0,
                    new Dictionary<string, string> { { "c", "gsaCA" } }));

            AssertServiceDefinition(
                certificateStatusDefinition,
                new ServiceDefinition(
                    "gsaCS",
                    new Uri("http://gsaCS.gsa.com:80"),
                    0,
                    new Dictionary<string, string>()));

            AssertServiceDefinition(
                commConfigDefinition,
                new ServiceDefinition(
                    "g2sCC",
                    new Uri("https://g2sCC.g2s.com:443"),
                    1,
                    new Dictionary<string, string>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WhenParseVendorSpecificInformationLongValueExpectException()
        {
            var value = new string(new char[256]);
            VendorSpecificInformation.Create(value);
        }

        [TestMethod]
        public void
            WhenParseVendorSpecificInformationValueWithCommConfigAndCertificateStatusServicesExpectValidDataModel()
        {
            var value =
                "g2sCC=shs:10.1.1.1:8080/g2s/services/G2SAPIServices+1|" +
                "gsaCS=tou:10.2.2.2/ocsp|" +
                "gsaCM=tsu:10.3.3.3/certsrv/mscep/^c=gsaCA|" +
                "gsaOA=30|gsaOR=600|gsaOO=14400";

            var info = VendorSpecificInformation.Create(value);

            var commConfigDefinitions = info.CommConfigDefinitions;
            var certificateManagerDefinitions = info.CertificateManagerDefinitions;
            var certificateStatusDefinitions = info.CertificateStatusDefinitions;

            Assert.AreEqual(1, commConfigDefinitions.Count);
            Assert.AreEqual(1, certificateManagerDefinitions.Count); // default
            Assert.AreEqual(1, certificateStatusDefinitions.Count);

            var commConfigDefinition = commConfigDefinitions.First();
            var certificateManagerDefinition = certificateManagerDefinitions.First();
            var certificateStatusDefinition = certificateStatusDefinitions.ElementAt(0);

            AssertServiceDefinition(
                commConfigDefinition,
                new ServiceDefinition(
                    "g2sCC",
                    new Uri("https://10.1.1.1:8080/g2s/services/G2SAPIServices"),
                    1,
                    new Dictionary<string, string>()));

            AssertServiceDefinition(
                certificateManagerDefinition,
                new ServiceDefinition(
                    "gsaCM",
                    new Uri("http://10.3.3.3/certsrv/mscep/"),
                    0,
                    new Dictionary<string, string> { { "c", "gsaCA" } }));

            AssertServiceDefinition(
                certificateStatusDefinition,
                new ServiceDefinition(
                    "gsaCS",
                    new Uri("http://10.2.2.2/ocsp"),
                    0,
                    new Dictionary<string, string>()));

            Assert.AreEqual(14400, info.OcspMinimumPeriodForOfflineMin);
            Assert.AreEqual(600, info.OcspReauthPeriodMin);
            Assert.AreEqual(30, info.OcspAcceptPrevGoodPeriodMin);
        }

        [TestMethod]
        public void WhenParseVendorSpecificInformationCustomParametersExpectValidDataModel()
        {
            var value = "abcXX=25|abcYY=35";

            var info = VendorSpecificInformation.Create(value);

            var certificateManagerDefinitions = info.CertificateManagerDefinitions;
            var certificateStatusDefinitions = info.CertificateStatusDefinitions;
            var commConfigDefinitions = info.CommConfigDefinitions;

            var ocspAcceptPrevGoodPeriodMin = info.OcspAcceptPrevGoodPeriodMin;
            var ocspMinimumPeriodForOfflineMin = info.OcspMinimumPeriodForOfflineMin;
            var ocspReauthPeriodMin = info.OcspReauthPeriodMin;

            // defaults
            Assert.AreEqual(1, certificateManagerDefinitions.Count);
            Assert.AreEqual(1, certificateStatusDefinitions.Count);
            Assert.AreEqual(1, commConfigDefinitions.Count);

            Assert.AreEqual(720, ocspAcceptPrevGoodPeriodMin);
            Assert.AreEqual(240, ocspMinimumPeriodForOfflineMin);
            Assert.AreEqual(600, ocspReauthPeriodMin);

            var customParameters = info.CustomParameters;
            Assert.AreEqual(2, customParameters.Count);

            var customParameterFirst = customParameters.ElementAt(0);
            var customParameterSecond = customParameters.ElementAt(1);

            AssertParameter(customParameterFirst, new Parameter("abcXX", "25"));
            AssertParameter(customParameterSecond, new Parameter("abcYY", "35"));
        }

        [TestMethod]
        public void WhenParseVendorSpecificInformationValuWithEscapeFormatExpectValidDataModel()
        {
            var value = "g2sCC%3Dtsu%3Acertmgr.casino.com%3A8080%2B5%5Ename%3Dvalue%7CgsaOO%3D15%7CabcXX%3D55";

            var info = VendorSpecificInformation.Create(value);

            var certificateManagerDefinitions = info.CertificateManagerDefinitions;
            var certificateStatusDefinitions = info.CertificateStatusDefinitions;
            var commConfigDefinitions = info.CommConfigDefinitions;

            var ocspAcceptPrevGoodPeriodMin = info.OcspAcceptPrevGoodPeriodMin;
            var ocspMinimumPeriodForOfflineMin = info.OcspMinimumPeriodForOfflineMin;
            var ocspReauthPeriodMin = info.OcspReauthPeriodMin;

            Assert.AreEqual(1, commConfigDefinitions.Count);

            // defaults
            Assert.AreEqual(1, certificateManagerDefinitions.Count);
            Assert.AreEqual(1, certificateStatusDefinitions.Count);

            Assert.AreEqual(720, ocspAcceptPrevGoodPeriodMin);
            Assert.AreEqual(600, ocspReauthPeriodMin);

            Assert.AreEqual(ocspMinimumPeriodForOfflineMin, 15);

            var commConfigDefinition = commConfigDefinitions.First();

            AssertServiceDefinition(
                commConfigDefinition,
                new ServiceDefinition(
                    "g2sCC",
                    new Uri("http://certmgr.casino.com:8080"), 
                    5,
                    new Dictionary<string, string> { { "name", "value" } }));

            var customParameters = info.CustomParameters;
            Assert.AreEqual(1, customParameters.Count);

            var customParameterFirst = customParameters.First();
            AssertParameter(customParameterFirst, new Parameter("abcXX", "55"));
        }

        private void AssertServiceDefinition(ServiceDefinition expected, ServiceDefinition actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Address, actual.Address);
            Assert.AreEqual(expected.HostId, actual.HostId);

            Assert.AreEqual(expected.ServiceParameters.Count, actual.ServiceParameters.Count);

            for (int i = 0; i < actual.ServiceParameters.Count; i++)
            {
                var expectedServiceParameter = expected.ServiceParameters.ElementAt(i);
                var actualServiceParameter = actual.ServiceParameters.ElementAt(i);

                Assert.AreEqual(expectedServiceParameter.Key, actualServiceParameter.Key);
                Assert.AreEqual(expectedServiceParameter.Value, actualServiceParameter.Value);
            }
        }

        private void AssertParameter(Parameter expected, Parameter actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Value, actual.Value);
        }
    }
}