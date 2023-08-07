namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using log4net;
    using System.Reflection;
    using GdkRuntime.V1;
    using Commands;
    using Contracts;

    public class SnappPresentationService : IPresentationServiceCallback
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly ICommandHandlerFactory _handlerFactory;

        public SnappPresentationService(ICommandHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public override RegisterPresentationResponse RegisterPresentation(
            RegisterPresentationRequest request)
        {
            Logger.Debug("RegisterPresentation entered");

            var registeredTypes = new PresentationOverrideTypes[request.Types_.Count];

            for (var i = 0; i < request.Types_.Count; i++)
            {
                registeredTypes[i] = request.Types_[i] switch
                {
                    PresentationType.PrintingCashoutTicket => PresentationOverrideTypes.PrintingCashoutTicket,
                    PresentationType.PrintingCashwinTicket => PresentationOverrideTypes.PrintingCashwinTicket,
                    PresentationType.TransferingInCredits => PresentationOverrideTypes.TransferingInCredits,
                    PresentationType.TransferingOutCredits => PresentationOverrideTypes.TransferingOutCredits,
                    PresentationType.JackpotHandpay => PresentationOverrideTypes.JackpotHandpay,
                    PresentationType.BonusJackpot => PresentationOverrideTypes.BonusJackpot,
                    PresentationType.CancelledCreditsHandpay => PresentationOverrideTypes.CancelledCreditsHandpay,
                    _ => throw new ArgumentOutOfRangeException(nameof(request), @"Unexpected presentation type registration")
                };
            }

            var command = new RegisterPresentation(request.Result, registeredTypes);

            _handlerFactory.Create<RegisterPresentation>().Handle(command);

            return new RegisterPresentationResponse { Result = command.Success };
        }
    }
}
