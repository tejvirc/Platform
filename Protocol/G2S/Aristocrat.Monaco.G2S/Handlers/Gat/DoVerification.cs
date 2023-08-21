namespace Aristocrat.Monaco.G2S.Handlers.Gat
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Storage;
    using Kernel.Contracts.Components;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class DoVerification : ICommandHandler<gat, doVerification>
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IGatService _gatService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoVerification" /> class.
        ///     Constructs a new instance using an egm and the GAT service.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="gatService">An instance of IGatService.</param>
        /// <param name="eventLift">Event lift.</param>
        public DoVerification(IG2SEgm egm, IGatService gatService, IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gat, doVerification> command)
        {
            var error = await Sanction.OnlyOwner<IGatDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            foreach (var verification in command.Command.verifyComponent)
            {
                var component = _gatService.GetComponent(verification.componentId);
                if (component == null)
                {
                    return new Error(ErrorCode.G2S_GAX006);
                }

                var supportedAlgorithms = _gatService.GetSupportedAlgorithms(component.Type);

                var algorithms = supportedAlgorithms.Select(
                    a => new algorithm
                    {
                        algorithmType = a.Type.ToG2SAlgorithmType(),
                        supportsOffsets = a.SupportsOffsets,
                        supportsSeed = a.SupportsSeed,
                        supportsSalt = a.SupportsSalt
                    }).ToList();

                var type = algorithms.FirstOrDefault(
                    t => t.algorithmType.Equals(
                        verification.algorithmType,
                        StringComparison.InvariantCultureIgnoreCase));

                if (type == null)
                {
                    return new Error(ErrorCode.G2S_GAX007);
                }

                if (type.supportsSeed && string.IsNullOrEmpty(verification.seed))
                {
                    return new Error(ErrorCode.G2S_GAX013);
                }

                if (!type.supportsSeed && !string.IsNullOrEmpty(verification.seed))
                {
                    return new Error(ErrorCode.G2S_GAX008);
                }

                if (!type.supportsSalt && verification.salt != null && verification.salt.Length > 0)
                {
                    return new Error(ErrorCode.G2S_GAX009);
                }

                if (!type.supportsOffsets && (verification.endOffset != -1 || verification.startOffset != 0))
                {
                    return new Error(ErrorCode.G2S_GAX010);
                }

                if (type.supportsOffsets && !ValidateOffset(component, verification))
                {
                    return new Error(ErrorCode.G2S_GAX010);
                }
            }

            if (command.Command.verificationId < 1)
            {
                return new Error(ErrorCode.G2S_GAX004);
            }

            //// TODO  G2S_GAX011, Verification Rejected – Queue Full.

            return await Task.FromResult<Error>(null);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gat, doVerification> command)
        {
            var response = command.GenerateResponse<verificationStatus>();

            if (!_gatService.HasVerificationId(command.Command.verificationId))
            {
                var device = _egm.GetDevice<IGatDevice>(command.IClass.deviceId);

                // TODO: This is wrong per the service, but right...
                // var args = new GetVerificationStatusArgs(command.Command.verificationId, command.IClass.deviceId);
                // _gatService.GetVerificationStatus(args);
                var components = command.Command.verifyComponent.Select(
                    c =>
                        new VerifyComponent(
                            c.componentId,
                            c.algorithmType.ToAlgorithmType(),
                            c.seed,
                            c.salt == null ? string.Empty : Convert.ToBase64String(c.salt),
                            c.startOffset,
                            c.endOffset));

                var args = new DoVerificationArgs(
                    command.Command.verificationId,
                    command.IClass.deviceId,
                    string.Empty,
                    components,
                    res =>
                    {
                        if (res.TransactionId != 0)
                        {
                            var vResult = new verificationResult
                            {
                                verificationId = command.Command.verificationId, transactionId = res.TransactionId
                            };

                            var arguments = new GetVerificationStatusByTransactionArgs(
                                vResult.transactionId,
                                vResult.verificationId);
                            var v = _gatService.GetVerificationStatus(arguments);

                            if (v != null)
                            {
                                vResult.componentResult =
                                    v.ComponentVerificationResults.Select(
                                        cvRes => new componentResult
                                        {
                                            componentId = cvRes.ComponentId,
                                            gatExec = cvRes.GatExec,
                                            verifyResult = cvRes.VerifyResult,
                                            verifyState = cvRes.State.ToG2SVerifyState()
                                        }).ToArray();
                                var verifySuccess = true;

                                if (vResult.componentResult.Any(
                                    cmpResult => cmpResult.verifyState == t_verifyStates.G2S_error))
                                {
                                    verifySuccess = false;
                                    _eventLift.Report(
                                        device,
                                        EventCode.G2S_GAE104,
                                        res.TransactionId,
                                        GetTransactionList(command, res.TransactionId, device));
                                }

                                if (verifySuccess)
                                {
                                    _eventLift.Report(
                                        device,
                                        EventCode.G2S_GAE103,
                                        res.TransactionId,
                                        GetTransactionList(command, res.TransactionId, device));
                                }
                            }

                            var vAck =
                                device.SendVerificationResult(vResult);

                            if (vAck != null)
                            {
                                foreach (var cAck in vAck.componentAck)
                                {
                                    var entity = _gatService.GetGatComponentVerificationEntity(
                                        cAck.componentId,
                                        res.VerificationId);
                                    if (entity != null)
                                    {
                                        if (cAck.passed && entity.State != ComponentVerificationState.Error)
                                        {
                                            entity.State = ComponentVerificationState.Passed;
                                            _gatService.UpdateGatComponentVerificationEntity(entity);
                                            _eventLift.Report(
                                                device,
                                                EventCode.G2S_GAE105,
                                                res.TransactionId,
                                                GetTransactionList(command, res.TransactionId, device));
                                        }
                                        else
                                        {
                                            entity.State = ComponentVerificationState.Failed;
                                            _gatService.UpdateGatComponentVerificationEntity(entity);
                                            _eventLift.Report(
                                                device,
                                                EventCode.G2S_GAE106,
                                                res.TransactionId,
                                                GetTransactionList(command, res.TransactionId, device));
                                        }
                                    }
                                }
                            }
                        }
                    },
                    transactionId =>
                        _eventLift.Report(
                            device,
                            EventCode.G2S_GAE101,
                            transactionId,
                            GetTransactionList(command, transactionId, device)),
                    transactionId =>
                        _eventLift.Report(
                            device,
                            EventCode.G2S_GAE102,
                            transactionId,
                            GetTransactionList(command, transactionId, device)));

                var result = _gatService.DoVerification(args);

                response.Command.transactionId = result.TransactionId;
                response.Command.verificationId = result.VerificationId;
                response.Command.componentStatus = result.ComponentStatuses.Select(
                        v => new componentStatus
                            {
                                componentId = v.ComponentId, verifyState = v.State.ToG2SVerifyState()
                            })
                    .ToArray();
            }
            else
            {
                var request = _gatService.GetVerificationRequestById(command.Command.verificationId);
                var args = new GetVerificationStatusByTransactionArgs(request.TransactionId, request.VerificationId);
                var status = _gatService.GetVerificationStatus(args);

                response.Command.transactionId = request.TransactionId;
                response.Command.verificationId = request.VerificationId;

                if (status.ComponentVerificationResults != null)
                {
                    response.Command.componentStatus = status.ComponentVerificationResults.Select(
                        v => new componentStatus
                        {
                            componentId = v.ComponentId, verifyState = v.State.ToG2SVerifyState()
                        }).ToArray();
                }
            }

            await Task.CompletedTask;
        }

        private transactionList GetTransactionList(
            ClassCommand<gat, doVerification> command,
            long transactionId,
            IGatDevice device)
        {
            var log = _gatService.GetLogForTransactionId(transactionId);

            if (log == null)
            {
                return null;
            }

            var gatLog = GatEnumExtensions.GetGatLog(log);

            var info = new transactionInfo
            {
                Item = gatLog, deviceId = command.IClass.deviceId, deviceClass = device.PrefixedDeviceClass()
            };

            var transList = new transactionList { transactionInfo = new[] { info } };

            return transList;
        }

        private bool ValidateOffset(Component component, c_verifyComponent verification)
        {
            if (component.Size == 0)
            {
                return true;
            }

            if (verification.startOffset < 0 ||
                verification.endOffset < -1 ||
                verification.startOffset > 0 && verification.startOffset > component.Size ||
                verification.endOffset > component.Size)
            {
                return false;
            }

            return true;
        }
    }
}