namespace Aristocrat.Monaco.G2S.Common.Transfer
{
    using System.Text.RegularExpressions;
    using Application.Contracts.Localization;
    using Ftp;
    using Http;
    using Localization.Properties;
    using Monaco.Common.Exceptions;

    /// <summary>
    /// 
    /// </summary>
    public class TransferFactory : ITransferFactory
    {
        private const string HttpProtocolRegEx = @"^((http[s]?):\/\/)+([^\s]*)$";
        private const string FtpProtocolRegEx = @"^(([s]?ftp[s]?):\/\/)+([^\s]*)$";
        
        /// <summary>
        /// returns the transfer service based on the string input
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public IProtocolTransferService GetTransferService(string location)
        {
            if (Regex.IsMatch(location, FtpProtocolRegEx))
            {
                return new FtpTransferService();
            }

            if (Regex.IsMatch(location, HttpProtocolRegEx))
            {
                return new HttpTransferService();
            }

            throw new CommandException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoFoundProtocolErrorMessage));
        }
    }
}