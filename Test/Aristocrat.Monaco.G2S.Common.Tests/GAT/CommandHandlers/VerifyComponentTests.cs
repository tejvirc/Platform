namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using Application.Contracts.Authentication;
    using Common.GAT.CommandHandlers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VerifyComponentTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfComponentIdIsNull()
        {
            new VerifyComponent(null, AlgorithmType.Crc16, "seed", "salt", 1, 1);
        }
    }
}
