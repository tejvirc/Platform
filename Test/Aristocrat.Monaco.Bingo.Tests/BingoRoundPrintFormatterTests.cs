namespace Aristocrat.Monaco.Bingo.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Test.Common;
    using Common;
    using Common.GameOverlay;
    using Common.Storage;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Tickets;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BingoRoundPrintFormatterTests
    {
        private BingoRoundPrintFormatter _target;
        private readonly Mock<ICentralProvider> _centralProvider = new();

        private readonly CentralTransaction _centralTransaction = new(1, DateTime.Now, 123, 1000, string.Empty, 1000, 1)
        {
            LogSequence = CentralTransactionLogSequenceNumber, TransactionId = CentralTransactionId
        };

        private readonly BingoPattern _pattern1 = new("Pattern 1", 1, CardSerial, Pattern1WinAmount, 25, 23, false, 0x25, 1);

        private readonly BingoPattern _pattern2 = new("Pattern 2", 2, CardSerial, Pattern2WinAmount, 25, 23, false, 0x25, 2);

        private readonly BingoPattern _coverallPattern = new(
            "Cover All",
            BingoConstants.CardCenterNumber,
            CardSerial,
            CoverallWinAmount,
            40,
            23,
            true,
            0xFF,
            3);

        private readonly List<int> _ballCallNumbers = new()
        {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            21,
            22,
            23,
            24,
            25,
            26,
            27,
            28,
            29,
            30,
            31,
            32,
            33,
            34,
            35,
            36,
            37,
            38,
            39,
            40,
            41,
            42,
            43,
            44,
            45,
            46,
            47,
            48,
            49,
            50,
            51,
            52,
            53,
            54,
            55,
            56,
            57,
            58,
            59,
            60,
            61,
            62,
            63,
            64,
            65,
            66,
            67,
            68,
            69,
            70,
            71,
            72,
            73,
            74,
            75
        };

        private readonly int[] _cardNumbers =
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 0, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25
        };

        private readonly string[] _cardHeader = { "B", "I", "N", "G", "O" };

        private const int CentralTransactionId = 12;
        private const long CentralTransactionLogSequenceNumber = 1;
        private const int CardSerial = 1234;
        private const int Pattern1WinAmount = 500;
        private const int Pattern2WinAmount = 200;
        private const int CoverallWinAmount = 1;
        private const int JoinBallIndex = 0;
        private const int MaxBallCallSize = 75;

        private const string CardSerialLabel = "Card Serial Number: {0}";
        private const string BallCallHeader = "Ball Call:";
        private const string CoverallPrizeBallLabel = "Game Ending Prize Ball: {0}";
        private const string CoverallPrizeLabel = "Game Ending Prize: {0}";
        private const string JoinBallLabel = "Join Ball: {0}";
        private const string TotalWinLabel = "Total Win: {0}";
        private const string TotalWinWinningGame = "$7.00";
        private const string TotalWinWinningGameCoverall = "$7.01";
        private const string TotalWinLosingGame = "$0.00";
        private const string TotalWinLosingGameCoverall = "$0.01";
        private const string CoverallPrize = "$0.01";
        private const string FreeSpace = "*";

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);

            _target = new BingoRoundPrintFormatter(_centralProvider.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Constructor_Fail()
        {
            _target = new BingoRoundPrintFormatter(null);
        }

        [TestMethod]
        public void ServiceTest()
        {
            _target = new BingoRoundPrintFormatter(_centralProvider.Object);

            Assert.AreEqual(typeof(BingoRoundPrintFormatter).ToString(), _target.Name);
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.AreEqual(typeof(IGameRoundPrintFormatter), _target.ServiceTypes.First());
        }

        [TestMethod]
        public void GetFormattedData_CentralTransaction_NoMatch()
        {
            var centralTransactions = new List<CentralTransaction> { _centralTransaction };

            _centralProvider.Setup(x => x.Transactions)
                .Returns(centralTransactions);

            var result = _target.GetFormattedData(2);

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetFormattedData_CentralTransaction_EmptyList()
        {
            _centralProvider.Setup(x => x.Transactions)
                .Returns(new List<CentralTransaction>());

            var result = _target.GetFormattedData(CentralTransactionLogSequenceNumber);

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetFormattedData_BingoTransaction_EmptyList()
        {
            var centralTransactions = new List<CentralTransaction> { _centralTransaction };

            _centralProvider.Setup(x => x.Transactions)
                .Returns(centralTransactions);

            var result = _target.GetFormattedData(CentralTransactionLogSequenceNumber);
            Assert.AreSame(string.Empty, result);
        }

        [DataRow(false, false, 40, DisplayName = "Losing Game in 40 balls")]
        [DataRow(true, false, 40, DisplayName = "Coverall, no patterns, in 40 balls")]
        [DataRow(false, true, 40, DisplayName = "Two patterns, no coverall, in 40 balls")]
        [DataRow(true, true, 75, DisplayName = "Two patterns, coverall, in 75 balls")]
        [DataTestMethod]
        public void GetFormattedData(
            bool gameEndWinClaimAccepted,
            bool winningGame,
            int ballCallSize)
        {
            var centralTransactions = new List<CentralTransaction> { _centralTransaction };
            _centralTransaction.Descriptions = new List<BingoGameDescription>
            {
                SetupBingoDescription(gameEndWinClaimAccepted, winningGame, ballCallSize)
            };

            var totalWin = string.Format(
                TotalWinLabel,
                winningGame
                    ? gameEndWinClaimAccepted
                        ? TotalWinWinningGameCoverall
                        : TotalWinWinningGame
                    : gameEndWinClaimAccepted
                        ? TotalWinLosingGameCoverall
                        : TotalWinLosingGame);
            var joinBall = string.Format(JoinBallLabel, _ballCallNumbers.ElementAt(JoinBallIndex));
            var coverallPrize = string.Format(CoverallPrizeLabel, CoverallPrize);
            var coverallBall = string.Format(CoverallPrizeBallLabel, _ballCallNumbers.ElementAt(ballCallSize - 1));
            var cardSerial = string.Format(CardSerialLabel, CardSerial);
            var splitDelimiters = new char[] { '\n', '\v', ',', '\t' };

            _centralProvider.Setup(x => x.Transactions)
                .Returns(centralTransactions);

            var result = _target.GetFormattedData(CentralTransactionLogSequenceNumber);
            var parsedResults = result.Split(splitDelimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim(' '))
                .Where(x => !string.IsNullOrWhiteSpace(x));

            var resultIndex = 0;
            Assert.AreNotEqual(string.Empty, result);
            Assert.IsTrue(parsedResults.Any());
            Assert.AreEqual(totalWin, parsedResults.ElementAt(resultIndex++));
            Assert.AreEqual(joinBall, parsedResults.ElementAt(resultIndex++));

            if (gameEndWinClaimAccepted)
            {
                Assert.AreEqual(coverallPrize, parsedResults.ElementAt(resultIndex++));
                Assert.AreEqual(coverallBall, parsedResults.ElementAt(resultIndex++));
            }

            Assert.AreEqual(BallCallHeader, parsedResults.ElementAt(resultIndex++));

            for (var ballIndex = 0; ballIndex < ballCallSize; ballIndex++)
            {
                var ball = _ballCallNumbers[ballIndex].ToString();

                if (ballIndex == JoinBallIndex)
                {
                    ball = $"({ball})";
                }

                if (gameEndWinClaimAccepted && ballIndex == ballCallSize - 1)
                {
                    ball = $"[{ball}]";
                }

                Assert.AreEqual(ball, parsedResults.ElementAt(resultIndex++));
            }

            Assert.AreEqual(cardSerial, parsedResults.ElementAt(resultIndex++));

            var parsedOnSpaceResults = string.Join(" ", parsedResults.Skip(resultIndex).Take(int.MaxValue)).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim(' '))
                .Where(x => !string.IsNullOrWhiteSpace(x));

            resultIndex = 0;

            foreach (var letter in _cardHeader)
            {
                Assert.AreEqual(letter, parsedOnSpaceResults.ElementAt(resultIndex++));
            }

            foreach (var number in _cardNumbers)
            {
                Assert.AreEqual(
                    number == BingoConstants.CardCenterNumber
                        ? FreeSpace
                        : number.ToString(),
                    parsedOnSpaceResults.ElementAt(resultIndex++));
            }
        }

        private BingoGameDescription SetupBingoDescription(
            bool gameEndWinClaimAccepted = false,
            bool winningGame = false,
            int ballCallSize = MaxBallCallSize)
        {
            var transaction = new BingoGameDescription
            {
                GameEndWinClaimAccepted = gameEndWinClaimAccepted,
                JoinBallIndex = JoinBallIndex + 1,
                BallCallNumbers =
                    _ballCallNumbers.GetRange(0, ballCallSize)
                        .Select(x => new BingoNumber(x, BingoNumberState.BallCallInitial)),
                Cards = new List<BingoCard> { new(CreateBingoNumbers(), CardSerial, 0x45, 0x45, false) },
                Patterns = new List<BingoPattern>()
            };

            if (winningGame)
            {
                transaction.Patterns = transaction.Patterns.Concat(new List<BingoPattern> { _pattern1, _pattern2 })
                    .ToList();
            }

            transaction.Patterns = transaction.Patterns.Append(_coverallPattern).ToList();

            return transaction;
        }

        private BingoNumber[,] CreateBingoNumbers()
        {
            var bingoNumbers = new BingoNumber[BingoConstants.BingoCardDimension, BingoConstants.BingoCardDimension];
            for (var row = 0; row < BingoConstants.BingoCardDimension; ++row)
            {
                for (var col = 0; col < BingoConstants.BingoCardDimension; ++col)
                {
                    bingoNumbers[row, col] = new BingoNumber(_cardNumbers[col + row * BingoConstants.BingoCardDimension], BingoNumberState.BallCallInitial);
                }
            }

            return bingoNumbers;
        }
    }
}