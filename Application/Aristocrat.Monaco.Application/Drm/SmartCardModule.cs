namespace Aristocrat.Monaco.Application.Drm
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using log4net;
    using Newtonsoft.Json;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Security;
    using SmartCard;

    internal class SmartCardModule : IProtectionModule
    {
        private const string DeveloperId = "DEVELOPER";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string[] OnBoardCardReaderNames = { "MK7 Smart Card", "ATA Mk7iICC 0" };

        private readonly string _smartCardKeyFile;

        private SmartCardReader _reader;
        private SmartCardConnection _connection;
        private ulong _sequenceNumber;

        private bool _disposed;

        public SmartCardModule(string smartCardKeyFile)
        {
            _smartCardKeyFile = smartCardKeyFile;
        }

        public IEnumerable<IToken> Tokens { get; private set; }

        public bool IsDeveloper { get; private set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task Initialize()
        {
            _reader = await GetReader();
            if (_reader == null)
            {
                throw new ProtectionModuleException("Failed to find smart card");
            }

            Logger.Info($"Connecting to reader {_reader.Name}");

            _connection = await _reader.Connect(ShareMode.Exclusive, Protocol.T0);

            if (_connection == null || !_connection.IsValid)
            {
                throw new ProtectionModuleException("Failed to connect to the smart card");
            }

            if (!Authenticate(_connection, _smartCardKeyFile))
            {
                throw new ProtectionModuleException("Failed to authenticate the smart card");
            }

            _sequenceNumber = GetSequenceNumber(_connection);

            Tokens = GetTokens();

            IsDeveloper = Tokens.All(l => l.Name == DeveloperId);
        }

        public bool IsConnected()
        {
            return Poll();
        }

        public bool DecrementCounter(IToken token, Counter counter, int value)
        {
            if (!token.Counters.TryGetValue(counter, out _))
            {
                Logger.Warn($"Unknown counter for: ({token.Id} - {counter})");

                return false;
            }

            var currentData = ReadData(token.Id, 0x02, Convert.ToByte(counter));
            if (currentData == null)
            {
                Logger.Warn($"Failed to read data for: ({token.Id} - {counter})");

                return false;
            }

            var current = ToUInt32(currentData);
            if (current == 0)
            {
                Logger.Warn($"Counter value is zero and cannot be decremented: ({token.Id} - {counter})");

                return false;
            }

            if (current < value)
            {
                value = current;
            }

            var data = new List<byte>(ToByteArray(++_sequenceNumber));

            data.AddRange(ToByteArray(token.Id));
            data.AddRange(ToByteArray(value));

            var command = new ApduCommand(
                0x90,
                0x26,
                Convert.ToByte(counter),
                0x00,
                data.ToArray(),
                0x08);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                --_sequenceNumber;
                Logger.Warn(
                    $"Error decrementing counter ({token.Id} - {counter}): {response.Status1}{response.Status2}");
                return false;
            }

            if (token is Token internalToken)
            {
                internalToken.InternalCounters[counter] = current - value;
            }

            Logger.Debug($"Counter decremented: ({token.Id} - {counter}) from {current} to {current - value}");

            return ++_sequenceNumber == ToUInt64(response.Data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // ReSharper disable once UseNullPropagation
                if (_connection != null)
                {
                    _connection.Dispose();
                }

                if (_reader != null)
                {
                    _reader.Error -= OnReaderError;
                    _reader.Dispose();
                }
            }

            _reader = null;
            _connection = null;

            _disposed = false;
        }

        private static async Task<SmartCardReader> GetReader()
        {
            var readers = await ReaderInformation.FindAll();

            var info = readers.FirstOrDefault(x => CheckOnBoardSmartCardReaderDevice(x.ReaderName));
            if (info == null)
            {
                return null;
            }

            var reader = await SmartCardReader.FromName(info.ReaderName);

            reader.Error += OnReaderError;

            return reader;
        }

        private static bool CheckOnBoardSmartCardReaderDevice(string name)
        {
            return OnBoardCardReaderNames.Any(
                x => string.Compare(
                    name,
                    x,
                    StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        private static void OnReaderError(object sender, SmartCardErrorArgs args)
        {
            Logger.Error($"Reader error - {args.Exception?.Message}");
        }

        private static bool Authenticate(SmartCardConnection connection, string smartCardKeyFile)
        {
            var modulusLength = GetModulusLength(connection);

            var publicKeyModulus = GetPublicKey(connection, (byte)modulusLength);

            var signature = GetSignature(connection);

            using (var rsa = ImportPublicKey(smartCardKeyFile))
            {
                return VerifyHash(rsa.ExportParameters(false), publicKeyModulus, signature);
            }
        }

        private static ushort GetModulusLength(SmartCardConnection connection)
        {
            // Command : class:0x90, inst:0x2a,le:3 (expected length) to retrieve the lengths of Mod and Exp for Public Key 

            var command = new ApduCommand(0x90, 0x2a, 0x00, 0x00, null, 0x03);

            var response = connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving modulus and exponent lengths");
            }

            var modulusLength = (ushort)((response.Data[0] << 8) | response.Data[1]);

            return modulusLength;
        }

        private static byte[] GetPublicKey(SmartCardConnection connection, byte modulusLength)
        {
            // Command : class:0x90, inst:0x2c,le:modLength (expected length) to retrieve Modulus for Public Key of Length modLength.

            var command = new ApduCommand(0x90, 0x2c, 0x00, 0x00, null, modulusLength);

            var response = connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving public key");
            }

            return response.Data;
        }

        private static byte[] GetSignature(SmartCardConnection connection)
        {
            // Command : class:0x90, inst:0x30 to retrieve Signature stored in Smart Card.

            var command = new ApduCommand(0x90, 0x30, 0x00, 0x00);

            var response = connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving signature");
            }

            return response.Data;
        }

        private static bool VerifyHash(RSAParameters parameters, byte[] signedData, byte[] signature)
        {
            var rsaCsp = new RSACryptoServiceProvider();
            var hash = SHA1.Create();

            rsaCsp.ImportParameters(parameters);

            return rsaCsp.VerifyHash(hash.ComputeHash(signedData), CryptoConfig.MapNameToOID("SHA1"), signature);
        }

        private static RSACryptoServiceProvider ImportPublicKey(string smartCardKeyFile)
        {
            var pem = File.ReadAllText(smartCardKeyFile);
            var pr = new PemReader(new StringReader(pem));

            if (!(pr.ReadObject() is RsaKeyParameters parameters))
            {
                throw new SecurityException($"Error retrieving Public Key from {smartCardKeyFile}");
            }

            var rsaParams = DotNetUtilities.ToRSAParameters(parameters);
            var csp = new RSACryptoServiceProvider();

            csp.ImportParameters(rsaParams);

            return csp;
        }

        private static ulong GetRandom()
        {
            using (var cryptoProvider = RandomNumberGenerator.Create())
            {
                var buffer = new byte[8];
                cryptoProvider.GetBytes(buffer);

                return BitConverter.ToUInt64(buffer, 0);
            }
        }

        private static ulong GetSequenceNumber(SmartCardConnection connection)
        {
            // Command : class:0x90, inst:0x1a initializes and returns a 8 Byte Sequence Number that must be XORd with our generated sequence number

            var sequence = GetRandom();

            var command = new ApduCommand(
                0x90,
                0x1a,
                0x00,
                0x00,
                ToByteArray(sequence),
                0x08);

            var response = connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                throw new ApduCommandException("Error retrieving sequence number");
            }

            // Per the spec
            sequence ^= ToUInt64(response.Data);

            return sequence;
        }

        private static ushort ToUInt16(byte[] bytes, int offset = 0)
        {
            var data = new byte[2];

            Array.Copy(bytes, offset, data, 0, data.Length);

            Array.Reverse(data);

            return BitConverter.ToUInt16(data, 0);
        }

        private static int ToUInt32(byte[] bytes, int offset = 0)
        {
            var data = new byte[4];

            Array.Copy(bytes, offset, data, 0, data.Length);

            Array.Reverse(data);

            return BitConverter.ToInt32(data, 0);
        }

        private static ulong ToUInt64(byte[] bytes, int offset = 0)
        {
            var data = new byte[8];

            Array.Copy(bytes, offset, data, 0, data.Length);

            Array.Reverse(data);

            return BitConverter.ToUInt64(data, 0);
        }

        private static byte[] ToByteArray(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);

            Array.Reverse(bytes);

            return bytes;
        }

        private static byte[] ToByteArray(int value)
        {
            var bytes = BitConverter.GetBytes(value);

            Array.Reverse(bytes);

            return bytes;
        }

        private static byte[] ToByteArray(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);

            Array.Reverse(bytes);

            return bytes;
        }

        private IEnumerable<IToken> GetTokens()
        {
            var tokens = new List<IToken>();

            var token = GetNextToken(true);
            while (token != null)
            {
                tokens.Add(token);

                token = GetNextToken();
            }

            return tokens;
        }

        private IToken GetNextToken(bool reset = false)
        {
            var command = new ApduCommand(
                0x90,
                0x1e,
                Convert.ToByte(reset),
                0x00,
                ToByteArray(++_sequenceNumber),
                0x88);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                --_sequenceNumber;
                Logger.Warn($"Error retrieving next SPT: {response.Status1}{response.Status2}");
                return null;
            }

            var tokenIdLength = response.Data.Length - 8;

            var tokenData = new byte[tokenIdLength];

            Array.Copy(response.Data, 0, tokenData, 0, tokenIdLength);

            var tokenName = Encoding.ASCII.GetString(tokenData);

            return ++_sequenceNumber != ToUInt64(response.Data, tokenIdLength) ? null : GetToken(tokenName);
        }

        private IToken GetToken(string tokenName)
        {
            var data = new List<byte>(ToByteArray(++_sequenceNumber));
            data.AddRange(Encoding.ASCII.GetBytes(tokenName));

            var command = new ApduCommand(
                0x90,
                0x1c,
                0x00,
                0x00,
                data.ToArray(),
                0x88);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                --_sequenceNumber;
                Logger.Warn($"Error retrieving SPT: {response.Status1}{response.Status2}");
                return null;
            }

            if (++_sequenceNumber != ToUInt64(response.Data, 8))
            {
                return null;
            }

            var token = new Token { Id = ToUInt16(response.Data), Name = tokenName, Locks = response.Data[6] };

            //var recordLength = ToUInt16(response.Data, 2);

            var dataLength = ToUInt16(response.Data, 4);
            if (dataLength > 1)
            {
                var currentData = ReadData(token.Id, 0x00, 0x00);

                try
                {
                    token.Data = JsonConvert.DeserializeObject<TokenData>(Encoding.UTF8.GetString(currentData, 0, currentData.Length));
                }
                catch (Exception e)
                {
                    // Don't fail due to invalid data here.  It can be restricted in the digital rights class if needed
                    token.Data = new TokenData();

                    Logger.Warn("Failed to parse token data from the smart card", e);
                }

                Logger.Debug($"Successfully read SPT data: {token.Name} - {currentData}");
            }

            var numCounters = response.Data[7];
            for (var counter = 1; counter <= numCounters; counter++)
            {
                var currentData = ReadData(token.Id, 0x02, Convert.ToByte(counter));
                if (currentData == null)
                {
                    Logger.Warn($"Failed to read data for: ({token.Id} - {counter})");
                    continue;
                }

                token.InternalCounters.Add((Counter)counter, ToUInt32(currentData));
            }

            Logger.Info(
                $"Successfully read token - Id:{token.Id} Name:{token.Name} Locks:{token.Locks} Counters: {token.Counters.Count()} [{string.Join(",", token.Counters.Select(c => $"{c.Key} - {c.Value}"))}]");

            return token;
        }

        private bool Poll()
        {
            // Command : class:0x90, inst:0x1a initializes and returns a 8 Byte Sequence Number that will be used to get Authorized Signature Data.

            var command = new ApduCommand(
                0x90,
                0x22,
                0x00,
                0x00,
                ToByteArray(++_sequenceNumber),
                0x08);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                --_sequenceNumber;
                Logger.Warn($"Error polling device: {response.Status1}{response.Status2}");
                return false;
            }

            return ++_sequenceNumber == ToUInt64(response.Data);
        }

        private byte[] ReadData(ushort index, byte type, byte value)
        {
            var data = new List<byte>(ToByteArray(++_sequenceNumber));

            data.AddRange(ToByteArray(index));
            data.Add(value);
            if (type == 0x00) // Appears to be required per the protocol
            {
                data.Add(type);
            }

            var command = new ApduCommand(
                0x90,
                0x20,
                type,
                0x00,
                data.ToArray(),
                0x88);

            var response = _connection.Transmit(command);

            if (response.Data == null || !response.IsStatusSuccess)
            {
                --_sequenceNumber;
                return null;
            }

            var returnDataLength = response.Data.Length - 8;

            var returnData = new byte[returnDataLength];

            Array.Copy(response.Data, 0, returnData, 0, returnDataLength);

            return ++_sequenceNumber == ToUInt64(response.Data, returnDataLength) ? returnData : null;
        }

        private class Token : IToken
        {
            public ushort Id { get; set; }

            public string Name { get; set; }

            public TokenData Data { get; set; }

            public ulong Locks { get; set; }

            public IReadOnlyDictionary<Counter, int> Counters => InternalCounters;

            public Dictionary<Counter, int> InternalCounters { get; } = new Dictionary<Counter, int>();
        }
    }
}