using Aristocrat.Monaco.Hhr.Client.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Aristocrat.Monaco.Hhr.Client.Tests.Communication
{
    [TestClass]
    public class EncryptionTests
    {
        [DataTestMethod]
        [DataRow("",    new byte[] { 1, 2, 3, 4, 5 },          new byte[] { 1, 2, 3, 4, 5 })]
        [DataRow("abc", new byte[] { 1, 2, 3, 4, 5 },          new byte[] { 168, 99, 116, 195, 40, 172, 196, 99 })]
        [DataRow("",    new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 }, new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 })]
        [DataRow("abc", new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 }, new byte[] { 97, 90, 45, 148, 122, 42, 223, 94, 236, 122, 238, 126, 33, 19, 125, 196 })]
        public async Task RoundTrip_Various_ShouldSucceed(string encryptionKey, byte[] inputBytes, byte[] outputBytes)
        {
            CryptoProvider cp = new CryptoProvider(encryptionKey);
            byte[] output = await cp.Encrypt(inputBytes);
            Assert.AreEqual(outputBytes.Length, output.Length);
            CollectionAssert.AreEqual(outputBytes, output);
            byte[] roundtrip = await cp.Decrypt(output);
            Assert.AreEqual(inputBytes.Length, roundtrip.Length);
            CollectionAssert.AreEqual(inputBytes, roundtrip);
        }

        [TestMethod]
        public async Task Decrypt_WrongKey_ShouldThrow()
        {
            CryptoProvider cp = new CryptoProvider("ghi");
            Func<Task> func = async () => await cp.Decrypt(new byte[] { 168, 99, 116, 195, 40, 172, 196, 99 });
            await Assert.ThrowsExceptionAsync<CryptographicException>(func);
        }
    }
}
