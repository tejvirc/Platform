namespace Aristocrat.Mgam.Client.Command
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Contains a list of supported commands.
    /// </summary>
    public class SupportedCommands
    {
        private static readonly Dictionary<int, CommandInfo> Commands = new Dictionary<int, CommandInfo>
        {
            {
                (int)CommandCode.Exit,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.Exit,
                    Description = "EXIT",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.Shutdown,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.Shutdown,
                    Description = "SHUTDOWN",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.Reboot,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.Reboot,
                    Description = "REBOOT",
                    ParameterName = "Instructions",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.MalformedMessage,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.MalformedMessage,
                    Description = "MALFORMED_MESSAGE",
                    ParameterName = "Error description",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController
                }
            },
            {
                (int)CommandCode.Play,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.Play,
                    Description = "PLAY",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.SignMessage,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.SignMessage,
                    Description = "SIGN_MESSAGE",
                    ParameterName = "Text Message",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.LogOffPlayer,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.LogOffPlayer,
                    Description = "LOGOFF_PLAYER",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.ProgressiveWinner,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.ProgressiveWinner,
                    Description = "PROGRESSIVE_WINNER",
                    ParameterName = "Text Message",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController
                }
            },
            {
                (int)CommandCode.Lock,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.Lock,
                    Description = "LOCK",
                    ParameterName = "Text Message",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.UpdateMeters,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.UpdateMeters,
                    Description = "UPDATE_METERS",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.ClearMeters,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.ClearMeters,
                    Description = "CLEAR_METERS",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.PlayExistingSession,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.PlayExistingSession,
                    Description = "PLAY_EXISTING_SESSION",
                    ParameterName = "Session ID",
                    Type = CommandValueType.Integer,
                    DefaultValue = 0,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController
                }
            },
            {
                (int)CommandCode.ComputeChecksum,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.ComputeChecksum,
                    Description = "COMPUTE_CHECKSUM",
                    ParameterName = "Seed",
                    Type = CommandValueType.Integer,
                    DefaultValue = 0,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.ClearLock,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.ClearLock,
                    Description = "CLEAR_LOCK",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            },
            {
                (int)CommandCode.Unplay,
                new CommandInfo
                {
                    CommandId = (int)CommandCode.Unplay,
                    Description = "UNPLAY",
                    Type = CommandValueType.String,
                    ControlType = CommandControlType.EditBox,
                    AccessType = CommandAccessType.SiteController | CommandAccessType.Management
                }
            }
        };

        /// <summary>
        ///     Gets a list of supported commands.
        /// </summary>
        /// <returns><see cref="CommandInfo"/> enumeration.</returns>
        public static IEnumerable<CommandInfo> Get() => Commands.Values;

        /// <summary>
        ///     Adds a custom attributes.
        /// </summary>
        public static void Add(CommandInfo command)
        {
            if (string.IsNullOrWhiteSpace(command.Description))
            {
                throw new ArgumentNullException(nameof(command), @"Description cannot be null");
            }

            if (command.DefaultValue == null)
            {
                throw new ArgumentNullException(nameof(command), @"DefaultValue cannot be null");
            }

            if (command.AccessType == CommandAccessType.None)
            {
                throw new ArgumentNullException(nameof(command), @"AccessType cannot be None");
            }

            if (Commands.ContainsKey(command.CommandId))
            {
                throw new ArgumentException($"{command.CommandId} command already exists");
            }

            Commands.Add(command.CommandId, command);
        }
    }
}
