namespace Aristocrat.Monaco.Application.EKey
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Kernel;
    using log4net;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Security;
    using SmartCard;

    /// <summary>
    ///     Base EKey smart card program.
    /// </summary>
    internal abstract class EKeyProgram
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private SmartCardConnection _connection;
        private readonly IPropertiesManager _properties;
        protected abstract string[] GetAuthTokens();

        /// <summary>
        ///     Initializes a new instance of the <see cref="EKeyProgram" /> class.
        /// </summary>
        protected EKeyProgram()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EKeyProgram" /> class.
        /// </summary>
        /// <param name="properties"></param>
        private EKeyProgram(IPropertiesManager properties)
        {
            _properties = properties;
        }

        /// <summary>
        ///     Runs a smart card program on a smart card.
        /// </summary>
        /// <param name="connection">A <see cref="SmartCardConnection"/> reference.</param>
        /// <param name="cancellation">A cancellation token to notify to cancel operations.</param>
        /// <returns>A value that indicates whether the program ran successfully.</returns>
        public virtual bool Run(SmartCardConnection connection, CancellationToken cancellation)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            try
            {
                var (modulusLength, exponentLength) = GetModulusAndExponentLengths();

                var publicKeyModulus = GetPublicKeyModulus((byte)modulusLength);

                var signature = GetSignature();

                var smartCardKeyFile = _properties.GetValue(KernelConstants.SmartCardKey, string.Empty);
#if !(RETAIL)
                if (string.IsNullOrEmpty(smartCardKeyFile))
                {
                    return true;
                }
#endif

                using (var rsa = ImportPublicKey(smartCardKeyFile))
                {
                    if (!VerifyHash(rsa.ExportParameters(false), publicKeyModulus, signature))
                    {
                        return false;
                    }
                }

                var verified = false;
                
                foreach (var token in GetAuthTokens())
                {
                    var sequenceNumber = GetSequenceNumber();

                    var authSignatureData = GetAuthSignatureData(sequenceNumber, token);

                    var authSignature = GetAuthSignature();

                    var exponent = GetExponent(exponentLength);

                    using (var rsa = new RSACryptoServiceProvider())
                    {
                        rsa.ImportParameters(new RSAParameters { Modulus = publicKeyModulus, Exponent = exponent });

                        if (VerifyHash(rsa.ExportParameters(false), authSignatureData, authSignature))
                        {
                            verified = true;
                            break;
                        }
                    }
                }

                if (!verified)
                {
                    Logger.Error("Authorization signatures did not match");
                }

                return verified;
            }
            catch (SmartCardException ex)
            {
                Logger.Error($"Error occurred while verifying eKey, {ex.Message}", ex);
            }

            return false;
        }

        private (ushort, byte) GetModulusAndExponentLengths()
        {
            // Command : class:0x90, inst:0x2a,le:3 (expected length) to retrieve the lengths of Mod and Exp for Public Key 

            var command = new ApduCommand(0x90, 0x2a, 0x00, 0x00, null, 0x03);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving modulus and exponent lengths");
            }

            var modulusLength = (ushort)((response.Data[0] << 8) | response.Data[1]);
            var exponentLength = response.Data[2];

            return (modulusLength, exponentLength);
        }

        private byte[] GetPublicKeyModulus(byte modulusLength)
        {
            // Command : class:0x90, inst:0x2c,le:modLength (expected length) to retrieve Modulus for Public Key of Length modLength.

            var command = new ApduCommand(0x90, 0x2c, 0x00, 0x00, null, modulusLength);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving public key");
            }

            return response.Data;
        }

        private byte[] GetSignature()
        {
            // Command : class:0x90, inst:0x30 to retrieve Signature stored in Smart Card.

            var command = new ApduCommand(0x90, 0x30, 0x00, 0x00);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving signature");
            }

            return response.Data;
        }

        private byte[] GetSequenceNumber()
        {
            // Command : class:0x90, inst:0x1a initializes and returns a 8 Byte Sequence Number that will be used to get Authorized Signature Data.

            var command = new ApduCommand(
                0x90,
                0x1a,
                0x00,
                0x00,
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                0x08);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving sequence number");
            }

            response.Data[response.Data.Length - 1]++;

            return response.Data;
        }

        private byte[] GetAuthSignatureData(byte[] sequenceNumber, string token)
        {
            // Command : class:0x90, inst:0x1c, data: sequence Number + Authorizing Token to get Authorized Signature Data.

            var data = new List<byte>(sequenceNumber);
            data.AddRange(Encoding.ASCII.GetBytes(token));

            var command = new ApduCommand(0x90, 0x1C, 0x00, 0x00, data.ToArray());

            var response = _connection.Transmit(command);
            
            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving authorization signature data");
            }

            var commandBytes = command.GetBytes();

            var signatureData = new byte[commandBytes.Length + response.Data.Length  + ApduResponse.StatusByteLength];

            commandBytes.CopyTo(signatureData, 0);
            response.Data.CopyTo(signatureData, commandBytes.Length);
            new byte[] { 0x90, 0x00 }.CopyTo(signatureData, commandBytes.Length + response.Data.Length);
            return signatureData;
        }

        private byte[] GetAuthSignature()
        {
            // Command : class:0x90, inst:0x1c, data: sequence Number + Authorizing Token to get Authorized Signature Data.

            var command = new ApduCommand(0x90, 0x32, 0x00, 0x00);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving authorization signature");
            }
            return response.Data;
        }

        private byte[] GetExponent(byte exponentLength)
        {
            // command : class:0x90, inst:0x2e, Le: expected Length is command block for getting Exponent for Public Key from Smart Card

            var command = new ApduCommand(0x90, 0x2e, 0x00, 0x00, null, exponentLength);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving exponent for Public Key");
            }
            return response.Data;
        }

        private static RSACryptoServiceProvider ImportPublicKey(string smartCardKeyFile)
        {
            var pem = File.ReadAllText(smartCardKeyFile);
            var pr = new PemReader(new StringReader(pem));
            if (!(pr.ReadObject() is RsaKeyParameters publicKey))
            {
                throw new ApduCommandException($"Error retrieving Public Key from {smartCardKeyFile}");
            }

            var rsaParams = DotNetUtilities.ToRSAParameters(publicKey);
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(rsaParams);
            return csp;
        }

        private static bool VerifyHash(RSAParameters parameters, byte[] signedData, byte[] signature)
        {
            var rsaCsp = new RSACryptoServiceProvider();
            var hash = SHA1.Create();

            rsaCsp.ImportParameters(parameters);

            return rsaCsp.VerifyHash(hash.ComputeHash(signedData), CryptoConfig.MapNameToOID("SHA1"), signature);
        }
    }
}
