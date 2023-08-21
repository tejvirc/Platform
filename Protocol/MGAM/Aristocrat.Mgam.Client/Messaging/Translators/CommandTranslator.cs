namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System;

    /// <summary>
    ///     Converts <see cref="T:Aristocrat.Mgam.Client.Protocol.Command"/> to <see cref="T:Aristocrat.Mgam.Client.Messaging.Command"/> message.
    /// </summary>
    public class CommandTranslator : MessageTranslator<Protocol.Command>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.Command message)
        {
            switch ((CommandCode)message.CommandID.Value)
            {
                case CommandCode.Exit:
                    return new Exit();

                case CommandCode.Shutdown:
                    return new Shutdown();

                case CommandCode.Reboot:
                    return new Reboot
                    {
                        IsClearLocks = string.CompareOrdinal(message.Parameter.Value, "Clear Lock") == 0
                    };

                case CommandCode.MalformedMessage:
                    return new MalformedMessage { ErrorDescription = message.Parameter.Value };

                case CommandCode.Play:
                    return new Play();

                case CommandCode.SignMessage:
                    return new Sign { Message = message.Parameter.Value };

                case CommandCode.LogOffPlayer:
                    return new LogoffPlayer();

                case CommandCode.ProgressiveWinner:
                    return new ProgressiveWinner { Message = message.Parameter.Value };

                case CommandCode.Lock:
                    return new Lock { Message = message.Parameter.Value };

                case CommandCode.UpdateMeters:
                    return new UpdateMeters();

                case CommandCode.ClearMeters:
                    return new ClearMeters();

                case CommandCode.PlayExistingSession:
                    if (!int.TryParse(message.Parameter.Value, out var sessionId))
                    {
                        throw new InvalidOperationException($"Invalid Session ID ({sessionId}) for PLAY_EXISTING_SESSION command");
                    }
                    return new PlayExistingSession { SessionId = sessionId };

                case CommandCode.ComputeChecksum:
                    if (string.IsNullOrEmpty(message.Parameter.Value))
                    {
                        return new ComputeChecksum { Seed = 0 };
                    }

                    if (!int.TryParse(message.Parameter.Value, out var seed))
                    {
                        throw new InvalidOperationException($"Invalid Seed ({seed}) for COMPUTE_CHECKSUM command");
                    }

                    return new ComputeChecksum { Seed = seed };

                case CommandCode.ClearLock:
                    return new ClearLock();

                case CommandCode.Unplay:
                    return new Unplay();

                default:
                    return new CustomCommand
                    {
                        CommandId = message.CommandID.Value, CommandParameter = message.Parameter.Value
                    };
            }
        }
    }
}
