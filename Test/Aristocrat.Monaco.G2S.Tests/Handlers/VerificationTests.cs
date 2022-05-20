namespace Aristocrat.Monaco.G2S.Tests.Handlers
{
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class VerificationTests
    {
        internal static async Task VerifyChecksForNoDevice<TClass, TCommand>(ICommandHandler<TClass, TCommand> handler)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler);

            Assert.IsNotNull(error);
            Assert.AreEqual(error.Code, TestConstants.InvalidDeviceErrorCode);
        }

        internal static async Task VerifyChecksTimeToLive<TClass, TCommand>(ICommandHandler<TClass, TCommand> handler)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            Assert.IsNotNull(handler);

            var command = ClassCommandUtilities.CreateExpiredClassCommand<TClass, TCommand>(
                TestConstants.HostId,
                TestConstants.EgmId);

            var error = await handler.Verify(command);

            Assert.IsNotNull(error);
            Assert.AreEqual(error.Code, TestConstants.TimeToLiveExpiredErrorCode);
        }

        internal static async Task VerifyChecksForOwner<TClass, TCommand>(ICommandHandler<TClass, TCommand> handler)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler);

            Assert.IsNotNull(error);
            Assert.AreEqual(error.Code, TestConstants.RestrictedToOwnerErrorCode);
        }

        internal static async Task VerifyChecksForOwnerAndGuests<TClass, TCommand>(
            ICommandHandler<TClass, TCommand> handler)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler);

            Assert.IsNotNull(error);
            Assert.AreEqual(error.Code, TestConstants.RestrictedToOwnerAndGuestsErrorCode);
        }

        internal static async Task VerifyDeniesGuests<TClass, TCommand>(ICommandHandler<TClass, TCommand> handler)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler);

            Assert.IsNotNull(error);
            Assert.AreEqual(error.Code, TestConstants.RestrictedToOwnerErrorCode);
        }

        internal static async Task VerifyAllowsGuests<TClass, TCommand>(ICommandHandler<TClass, TCommand> handler)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler);

            Assert.IsNull(error);
        }

        internal static async Task VerifyAllowsGuests<TClass, TCommand>(
            ICommandHandler<TClass, TCommand> handler,
            ClassCommand<TClass, TCommand> command)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler, command);

            Assert.IsNull(error);
        }

        internal static async Task VerifyCanSucceed<TClass, TCommand>(ICommandHandler<TClass, TCommand> handler)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler);

            Assert.IsNull(error);
        }

        internal static async Task VerifyCanSucceed<TClass, TCommand>(
            ICommandHandler<TClass, TCommand> handler,
            ClassCommand<TClass, TCommand> command)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler, command);

            Assert.IsNull(error);
        }

        internal static async Task VerifyReturnsError<TClass, TCommand>(
            ICommandHandler<TClass, TCommand> handler,
            string errorCode)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler);

            Assert.IsNotNull(error);
            Assert.AreEqual(errorCode, error.Code);
        }

        internal static async Task VerifyReturnsError<TClass, TCommand>(
            ICommandHandler<TClass, TCommand> handler,
            ClassCommand<TClass, TCommand> command,
            string errorCode)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var error = await DoVerification(handler, command);

            Assert.IsNotNull(error);
            Assert.AreEqual(errorCode, error.Code);
        }

        private static Task<Error> DoVerification<TClass, TCommand>(ICommandHandler<TClass, TCommand> handler)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var command = ClassCommandUtilities.CreateClassCommand<TClass, TCommand>(
                TestConstants.HostId,
                TestConstants.EgmId);

            return DoVerification(handler, command);
        }

        private static Task<Error> DoVerification<TClass, TCommand>(
            ICommandHandler<TClass, TCommand> handler,
            ClassCommand<TClass, TCommand> command)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            Assert.IsNotNull(handler);

            return handler.Verify(command);
        }
    }
}