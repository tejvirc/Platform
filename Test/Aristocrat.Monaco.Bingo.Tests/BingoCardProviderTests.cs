namespace Aristocrat.Monaco.Bingo.Tests
{
    using System;
    using Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BingoCardProviderTests
    {
        private readonly MersenneTwisterRng _mersenneTwister = new();
        private BingoCardProvider _target;

        [TestInitialize]
        public void Initialize()
        {
            _target = new BingoCardProvider(_mersenneTwister);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTest()
        {
            _target = new BingoCardProvider(null);
        }

        [TestMethod]
        public void GetCardBySerial()
        {
            // note: the serial# and card values were taken from an Ovation game replay
            var card = _target.GetCardBySerial(451681945);

            // expected card numbers
            var expected = new[,]
            {
                { 11, 22, 37, 59, 63 },
                { 07, 27, 31, 53, 68 },
                { 09, 30, 00, 49, 73 },
                { 14, 29, 43, 55, 67 },
                { 12, 23, 45, 56, 75 }
            };

            for (var row = 0; row < BingoConstants.BingoCardDimension; row++)
            {
                for (var col = 0; col < BingoConstants.BingoCardDimension; col++)
                {
                    Assert.AreEqual(expected[row, col], card.Numbers[row, col].Number);
                }
            }
        }
    }
}
