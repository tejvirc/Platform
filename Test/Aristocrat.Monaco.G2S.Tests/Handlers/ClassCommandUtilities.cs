namespace Aristocrat.Monaco.G2S.Tests.Handlers
{
    using Aristocrat.G2S;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using Test.Common.UnitTesting;

    internal static class ClassCommandUtilities
    {
        public static ClassCommand<TClass, TCommand> CreateClassCommand<TClass, TCommand>(int hostId, string egmId)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            return CreateClassCommand<TClass, TCommand>(hostId, egmId, hostId);
        }

        public static ClassCommand<TClass, TCommand> CreateClassCommand<TClass, TCommand>(
            int hostId,
            string egmId,
            int deviceId)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var valid = new TClass
            {
                deviceId = deviceId,
                timeToLive = TestConstants.TimeToLive,
                dateTime = DateTime.UtcNow
            };

            var obj = new PrivateObject(
                typeof(ClassCommand<TClass, TCommand>),
                valid,
                new TCommand(),
                hostId,
                egmId);

            var command = (ClassCommand<TClass, TCommand>)obj.Target;

            command.Received = DateTime.UtcNow;

            return command;
        }

        public static ClassCommand<TClass, TCommand> CreateExpiredClassCommand<TClass, TCommand>(
            int hostId,
            string egmId)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            return CreateExpiredClassCommand<TClass, TCommand>(hostId, egmId, hostId);
        }

        public static ClassCommand<TClass, TCommand> CreateExpiredClassCommand<TClass, TCommand>(
            int hostId,
            string egmId,
            int deviceId)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var expired = new TClass
            {
                deviceId = deviceId,
                timeToLive = TestConstants.TimeToLive,
                dateTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(TestConstants.TimeToLive + 1)
            };

            var obj = new PrivateObject(
                typeof(ClassCommand<TClass, TCommand>),
                expired,
                new TCommand(),
                hostId,
                egmId);

            var command = (ClassCommand<TClass, TCommand>)obj.Target;

            command.Received = DateTime.UtcNow - TimeSpan.FromMilliseconds(TestConstants.TimeToLive + 1);

            return command;
        }

        public static ClassCommand<TClass, TCommand> CreateClassCommand<TClass, TCommand>(
            TClass cls,
            int hostId,
            string egmId)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var obj = new PrivateObject(typeof(ClassCommand<TClass, TCommand>), cls, new TCommand(), hostId, egmId);

            return (ClassCommand<TClass, TCommand>)obj.Target;
        }

        public static ClassCommand<TClass, TCommand> CreateClassCommand<TClass, TCommand>(
            TCommand command,
            int hostId,
            string egmId)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            return CreateClassCommand<TClass, TCommand>(command, hostId, egmId, hostId);
        }

        public static ClassCommand<TClass, TCommand> CreateClassCommand<TClass, TCommand>(
            TCommand command,
            int hostId,
            string egmId,
            int deviceId)
            where TClass : IClass, new()
            where TCommand : ICommand, new()
        {
            var obj = new PrivateObject(
                typeof(ClassCommand<TClass, TCommand>),
                new TClass { deviceId = deviceId },
                command,
                hostId,
                egmId);

            return (ClassCommand<TClass, TCommand>)obj.Target;
        }
    }
}