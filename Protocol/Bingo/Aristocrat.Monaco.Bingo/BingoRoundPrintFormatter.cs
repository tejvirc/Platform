namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Common;
    using Common.Storage;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Tickets;
    using Localization.Properties;

    /// <inheritdoc cref="IGameRoundPrintFormatter"/>
    public class BingoRoundPrintFormatter : IGameRoundPrintFormatter
    {
        private readonly ICentralProvider _centralProvider;
        private static readonly string BallCallHeader = $"Ball Call: {LineBreak}";
        private static readonly string CardSerialNumber = $"Card Serial Number: {{0}}{LineBreak}";
        private static readonly char[] BallCallDelimiter = { ',', ' ' };

        private static readonly string HeaderCells = string.Join(
            CardCellDelimiter.ToString(),
            BingoConstants.DefaultCardTitle.Where(x => !char.IsWhiteSpace(x))
                .Select(x => x.ToString().PadRight(MaxBingoContentChars, PadCharacter)));

        private const string CoverallBallFormat = "[{0}]"; 
        private const string JoinBallFormat = "({0})"; 
        private const string BallCallFormat = "{0}, ";
        private const string FreeSpace = "*";
        private const char CardCellDelimiter = ' ';
        private const char PadCharacter = ' ';
        private const char LineBreak = '\n';
        private const char SectionBreak = '\v';
        private const int MaxPrintWidth = 45;
        private const int MaxBingoContentChars = 2;

        public BingoRoundPrintFormatter(ICentralProvider centralProvider)
        {
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
        }
        
        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameRoundPrintFormatter) };

        public void Initialize()
        {
        }

        public string GetFormattedData(long logSequenceNumber)
        {
            if (_centralProvider.Transactions.SingleOrDefault(x => x.LogSequence == logSequenceNumber)
                ?.Descriptions.FirstOrDefault() is not BingoGameDescription description)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            FormatGameData(description, builder);
            FormatBallCall(description, builder);
            FormatBingoCards(description, builder);
            return builder.ToString();
        }

        private static void FormatGameData(BingoGameDescription description, StringBuilder builder)
        {
            var gameEndWinCash = description.GameEndWinClaimAccepted
                ? description.Patterns.Single(x => x.IsGameEndWin).WinAmount
                : 0;
            var totalWinCash = description.Patterns.Where(x => description.GameEndWinClaimAccepted || !x.IsGameEndWin)
                .Sum(x => x.WinAmount);

            AddMeterLine(ResourceKeys.TotalWin, totalWinCash.CentsToDollars().FormattedCurrencyString(), builder);
            AddMeterLine(
                ResourceKeys.JoinBallLabel,
                description.BallCallNumbers.Select(x => x.Number).ElementAt(description.JoinBallIndex - 1),
                builder);
            if (!description.GameEndWinClaimAccepted)
            {
                return;
            }

            AddMeterLine(
                ResourceKeys.GameEndingPrizeLabel,
                gameEndWinCash.CentsToDollars().FormattedCurrencyString(),
                builder);
            AddMeterLine(
                ResourceKeys.GameEndingPrizeBallLabel,
                description.BallCallNumbers.Select(x => x.Number).Last(),
                builder);
        }

        private static void AddMeterLine<T>(string resourceKey, T value, StringBuilder builder)
        {
            builder.Append(Localizer.For(CultureFor.Operator).GetString(resourceKey)).Append(": ").Append(value)
                .Append(LineBreak);
        }

        private static void FormatBallCall(BingoGameDescription description, StringBuilder builder)
        {
            builder.Append(SectionBreak).Append(BallCallHeader);
            var bingoNumbers = description.BallCallNumbers?.ToList();
            if (bingoNumbers is null || !bingoNumbers.Any())
            {
                return;
            }

            var ballCallLine = new StringBuilder();
            for (var ballIndex = 0; ballIndex < bingoNumbers.Count; ++ballIndex)
            {
                var format = BallCallFormat;

                // Update the format if this is the join ball.
                if (ballIndex == description.JoinBallIndex - 1)
                {
                    format = string.Format(format, JoinBallFormat);
                }

                // Update the format if this is the coverall ball.
                if (description.GameEndWinClaimAccepted && ballIndex == bingoNumbers.Count - 1)
                {
                    format = string.Format(format, CoverallBallFormat);
                }

                // Format the ball number.
                var formattedBall = string.Format(format, bingoNumbers[ballIndex].Number);

                // Create a new line if we've reached the character limit.
                if (ballCallLine.Length + formattedBall.Length >= MaxPrintWidth)
                {
                    builder.Append(ballCallLine).Append(LineBreak);
                    ballCallLine.Clear();
                }

                ballCallLine.Append(formattedBall);
            }

            builder.Append(ballCallLine.ToString().TrimEnd(BallCallDelimiter)).Append(SectionBreak);
        }
        
        private static void FormatBingoCards(BingoGameDescription description, StringBuilder builder)
        {
            foreach (var card in description.Cards)
            {
                // Add the card's serial number.
                builder.Append(SectionBreak).AppendFormat(CardSerialNumber, card.SerialNumber);

                builder.Append(LineBreak).Append(HeaderCells).Append(LineBreak);

                // Add the card numbers.
                for (var row = 0; row < BingoConstants.BingoCardDimension; ++row)
                {
                    for (var col = 0; col < BingoConstants.BingoCardDimension; col++)
                    {
                        var number = row * BingoConstants.BingoCardDimension + col == BingoConstants.FreeSpaceIndex
                            ? FreeSpace
                            : card.Numbers[row, col].Number.ToString();
                        builder.Append(number.PadRight(MaxBingoContentChars, PadCharacter)).Append(CardCellDelimiter);
                    }

                    builder.Append(LineBreak);
                }
            }
        }
    }
}
