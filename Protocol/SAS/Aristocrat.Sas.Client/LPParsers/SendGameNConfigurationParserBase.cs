namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Client;
    using LongPollDataClasses;

    public class SendGameNConfigurationParserBase : SasLongPollParser<LongPollMachineIdAndInfoResponse, LongPollGameNConfigurationData>
    {
        private const int GameOptionsLength = 2;
        private const int AlternateIdLength = 3;
        private const int PaytableIdLength = 6;

        /// <summary>
        ///     Instantiates a new instance of the SendGameNConfigurationParserBase class
        /// </summary>
        protected SendGameNConfigurationParserBase(LongPoll command, SasClientConfiguration configuration)
            : base(command)
        {
            Data.AccountingDenom = configuration.AccountingDenom;
        }

        /// <inheritdoc/>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var infoResponse = Handle(Data);
            if (infoResponse == null)
            {
                return NackLongPoll(command);
            }

            // address and command bytes
            var response = command.Take(2).ToList();
            response.AddRange(GetResponseData(infoResponse));

            return response;
        }

        protected virtual IReadOnlyCollection<byte> GetResponseData(LongPollMachineIdAndInfoResponse handlerResponse)
        {
            var response = new List<byte>();
            response.AddRange(Encoding.ASCII.GetBytes(handlerResponse.GameId));
            if (handlerResponse.AdditionalId.Length < AlternateIdLength)
            {
                response.AddRange(Encoding.ASCII.GetBytes(new string('0', AlternateIdLength - handlerResponse.AdditionalId.Length)));
            }

            response.AddRange(
                Encoding.ASCII.GetBytes(
                    handlerResponse.AdditionalId.Substring(
                        0,
                        Math.Min(handlerResponse.AdditionalId.Length, AlternateIdLength))));
            
            response.Add(handlerResponse.Denomination);
            response.Add(handlerResponse.MaxBet);
            response.Add(handlerResponse.ProgressiveGroup);
            response.AddRange(Utilities.ToBinary(handlerResponse.GameOptions, GameOptionsLength));
            response.AddRange(
                Encoding.ASCII.GetBytes(
                    handlerResponse.PaytableId.Substring(
                        0,
                        Math.Min(handlerResponse.PaytableId.Length, PaytableIdLength))));
            if (handlerResponse.PaytableId.Length < PaytableIdLength)
            {
                // Null terminate the string if we are less than the required amount
                response.AddRange(Encoding.ASCII.GetBytes(new string('\0', PaytableIdLength - handlerResponse.PaytableId.Length)));
            }

            response.AddRange(Encoding.ASCII.GetBytes(handlerResponse.TheoreticalRtpPercent));
            return response;
        }
    }
}
