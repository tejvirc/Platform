namespace Aristocrat.Bingo.Client
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public static class CertificateExtensions
    {
        public static string ConvertToPem(this X509Certificate2 certificate)
        {
            var builder = new StringBuilder();
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(
                Convert.ToBase64String(
                    certificate.Export(X509ContentType.Cert),
                    Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");
            return builder.ToString();
        }
    }
}