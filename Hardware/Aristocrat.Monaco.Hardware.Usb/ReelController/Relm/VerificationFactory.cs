namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Security.Cryptography;
    using Common.Cryptography;
    using RelmReels.Messages;
    using RelmReels.Services;

    internal class VerificationFactory : IVerificationServiceFactory
    {
        public HashAlgorithm GetHashAlgorithm(BitmapVerification verification)
        {
            return verification switch
            {
                BitmapVerification.CRC32 => new Crc32Algorithm(false),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
