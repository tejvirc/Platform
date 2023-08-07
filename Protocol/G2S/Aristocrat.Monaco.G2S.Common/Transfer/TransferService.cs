namespace Aristocrat.Monaco.G2S.Common.Transfer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using log4net;

    /// <summary>
    ///     Transfer service implementation
    /// </summary>
    /// <seealso cref="ITransferService" />
    public class TransferService : ITransferService
    {
        private readonly ITransferFactory _transferFactory;
      
        private const int TransferRetryMax = 3;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferFactory"></param>
        public TransferService(ITransferFactory transferFactory)
        {
            _transferFactory = transferFactory;
        }

        /// <inheritdoc />
        public void Upload(
            string packageId,
            string destinationLocation,
            string transferParameters,
            Stream sourceStream,
            CancellationToken ct)
        {
            var transferService = _transferFactory.GetTransferService(destinationLocation); 
            TransferRetry(packageId,() => transferService.Upload(destinationLocation, transferParameters, sourceStream, ct), ct);
        }

        /// <inheritdoc />
        public void Download(
            string packageId,
            string downloadLocation,
            string transferParameters,
            Stream destinationStream,
            CancellationToken ct)
        { 
            var transferService = _transferFactory.GetTransferService(downloadLocation);
            TransferRetry(packageId,() => transferService.Download(downloadLocation, transferParameters, destinationStream, ct), ct);
        }

        /// <summary>
        ///     Gets parameter value by parameter name.
        /// </summary>
        /// <param name="parameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <param name="parameterName">Parameter name to get.</param>
        /// <returns>the parameter</returns>
        public static string GetParameterByName(string parameters, string parameterName)
        {
            var parametersDictionary = GetParameters(parameters);
            if (parametersDictionary.ContainsKey(parameterName))
            {
                return parametersDictionary[parameterName];
            }

            return string.Empty;
        }

        private void TransferRetry(string packageId, Action transferAction, CancellationToken ct)
        {
            var done = false;
            var attempt = 0;
            var hostError = false;
            var ioException = false;

            while (!done && attempt++ < TransferRetryMax && !ct.IsCancellationRequested)
            {
                try
                {
                    transferAction();
                    done = true;
                    Logger.Info("Transfer complete.");
                }
                catch (AggregateException ex)
                {
                    foreach (var exception in ex.Flatten().InnerExceptions)
                    {
                        if (exception is IOException)
                        {
                            ioException = true;
                        }
                        else
                        {
                            hostError = true;
                        }

                        Logger.Error($"Failed to transfer file packageId: {packageId} Exception: {exception}");
                    }
                }
                catch (IOException io)
                {
                    ioException = true;
                    Logger.Error($"Failed to transfer file packageId: {packageId} Exception: {io}");
                }
                catch (Exception ex)
                {
                    hostError = true;
                    Logger.Error($"Failed to transfer file packageId: {packageId} Exception: {ex}");
                }
            }

            if (!done)
            {
                if (ioException)
                {
                    throw new IOException("Unable to write file to disk.");
                }

                if (ct.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                if (hostError)
                {
                    throw new FtpServiceNotAvailableException("Host Error.");
                }

                throw new Exception();
            }
        }

        private static Dictionary<string, string> GetParameters(string parameters,char parametersSplitter = ';',char valueSplitter = '=')
        {
            if (string.IsNullOrEmpty(parameters) || !parameters.Contains(valueSplitter))
            {
                return new Dictionary<string, string>();
            }

            var items = parameters.Split(new[] { parametersSplitter }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Split(valueSplitter));

            var result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var item in items)
            {
                result[item[0]] = item[1];
            }

            return result;
        }
    }
}
