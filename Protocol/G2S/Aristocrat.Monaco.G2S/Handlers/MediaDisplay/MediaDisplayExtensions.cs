namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System;

    /// <summary>
    /// Extensions for G2S mediaDisplay messaging.
    /// </summary>
    public static class MediaDisplayExtensions
    {
        /// <summary>
        /// Convert <see cref="ScreenType"/> to <see cref="t_screenTypes"/>
        /// </summary>
        /// <param name="this">A <see cref="ScreenType"/> value</param>
        /// <returns>A <see cref="t_screenTypes"/> value</returns>
        public static t_screenTypes ToProtocolType(this ScreenType @this)
        {
            switch (@this)
            {
                case ScreenType.Primary:
                    return t_screenTypes.IGT_primary;
                case ScreenType.Secondary:
                    return t_screenTypes.IGT_secondary;
                case ScreenType.Glass:
                    return t_screenTypes.G2S_glass;
                case ScreenType.ButtonDeck:
                    return t_screenTypes.G2S_buttonDeck;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        /// Convert <see cref="DisplayType"/> to <see cref="t_mediaDisplayTypes"/>
        /// </summary>
        /// <param name="this">A <see cref="DisplayType"/> value</param>
        /// <returns>A <see cref="t_mediaDisplayTypes"/> value</returns>
        public static t_mediaDisplayTypes ToProtocolType(this DisplayType @this)
        {
            switch (@this)
            {
                case DisplayType.Scale:
                    return t_mediaDisplayTypes.IGT_scale;
                case DisplayType.Overlay:
                    return t_mediaDisplayTypes.IGT_overlay;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        /// Convert <see cref="DisplayPosition"/> to <see cref="t_mediaDisplayPositions"/>
        /// </summary>
        /// <param name="this">A <see cref="DisplayPosition"/> value</param>
        /// <returns>A <see cref="t_mediaDisplayPositions"/> value</returns>
        public static t_mediaDisplayPositions ToProtocolType(this DisplayPosition @this)
        {
            switch (@this)
            {
                case DisplayPosition.Left:
                    return t_mediaDisplayPositions.IGT_left;
                case DisplayPosition.Right:
                    return t_mediaDisplayPositions.IGT_right;
                case DisplayPosition.Top:
                    return t_mediaDisplayPositions.IGT_top;
                case DisplayPosition.Bottom:
                    return t_mediaDisplayPositions.IGT_bottom;
                case DisplayPosition.FullScreen:
                    return t_mediaDisplayPositions.IGT_fullScreen;
                case DisplayPosition.CenterScreen:
                    return t_mediaDisplayPositions.IGT_centerScreen;
                case DisplayPosition.Floating:
                    return t_mediaDisplayPositions.IGT_floating;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        /// Convert <see cref="MediaState"/> to <see cref="t_contentStates"/>
        /// </summary>
        /// <param name="this">A <see cref="MediaState"/> value</param>
        /// <returns>A <see cref="t_contentStates"/> value</returns>
        public static t_contentStates ToProtocolType(this MediaState @this)
        {
            switch (@this)
            {
                case MediaState.Pending:
                    return t_contentStates.IGT_contentPending;
                case MediaState.Loaded:
                    return t_contentStates.IGT_contentLoaded;
                case MediaState.Executing:
                    return t_contentStates.IGT_contentExecuting;
                case MediaState.Released:
                    return t_contentStates.IGT_contentReleased;
                case MediaState.Error:
                    return t_contentStates.IGT_contentError;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        /// Convert <see cref="MediaContentError"/> to integer
        /// </summary>
        /// <param name="this">A <see cref="MediaContentError"/> value</param>
        /// <returns>Integer equivalent</returns>
        public static int ToProtocolInt(this MediaContentError @this)
        {
            return (int)@this;
        }

        /// <summary>
        ///     Converts a <see cref="IMedia" /> instance to a <see cref="contentLog" />
        /// </summary>
        /// <param name="this">The <see cref="IMedia" /> instance to convert.</param>
        /// <param name="device">The mediaDisplay device</param>
        /// <returns>A <see cref="contentLog" /> instance.</returns>
        public static contentLog ToContentLog(this IMedia @this, IMediaDisplay device)
        {
            return new contentLog
            {
                logSequence = @this.LogSequence,
                deviceId = device.Id,
                transactionId = @this.TransactionId,
                contentId = @this.Id,
                mediaURI = @this.Address.ToString(),
                contentState = @this.State.ToProtocolType(),
                contentLoadedDateTimeSpecified = (@this.LoadTime != null),
                contentLoadedDateTime = @this.LoadTime ?? DateTime.MinValue,
                contentReleasedDateTimeSpecified = (@this.ReleaseTime != null),
                contentReleasedDateTime = @this.ReleaseTime ?? DateTime.MinValue,
                contentException = @this.ExceptionCode.ToProtocolInt()
            };
        }

        /// <summary>
        /// Create an empty <see cref="loadContent.authorizedEventList"/>
        /// </summary>
        /// <returns>The empty list</returns>
        public static loadContent.authorizedEventList.eventItem[] EmptyAuthorizedEventList()
        {
            return new loadContent.authorizedEventList.eventItem[0];
        }

        /// <summary>
        /// Create an empty <see cref="loadContent.authorizedCommandList"/>
        /// </summary>
        /// <returns>The empty list</returns>
        public static loadContent.authorizedCommandList.commandItem[] EmptyAuthorizedCommandList()
        {
            return new loadContent.authorizedCommandList.commandItem[0];
        }

        /// <summary>
        /// Create a list of implemented <see cref="c_mediaDisplayProfile.capabilitiesList.capabilityItem"/> media
        /// types that we support.
        /// </summary>
        /// <returns>List as <see cref="c_mediaDisplayProfile.capabilitiesList"/></returns>
        public static c_mediaDisplayProfile.capabilitiesList ImplementedCapabilitiesList()
        {
            return new c_mediaDisplayProfile.capabilitiesList()
            {
                capabilityItem1 = new[]
                {
                    new c_mediaDisplayProfile.capabilitiesList.capabilityItem
                    {
                        softwareType = t_softwareTypes.IGT_HTML,
                        fileExtension = "html"
                    }
                }
            };
        }

        /// <summary>
        /// Create a list of implemented <see cref="c_mediaDisplayProfile.localCommandList.localCommandItem"/> commands
        /// </summary>
        /// <returns>List as <see cref="c_mediaDisplayProfile.localCommandList"/></returns>
        public static c_mediaDisplayProfile.localCommandList ImplementedCommandList()
        {
            return new c_mediaDisplayProfile.localCommandList()
            {
                localCommandItem1 = new[]
                {
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Comms",
                        commandElement = "commsOnLine"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Comms",
                        commandElement = "heartbeat"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Comms",
                        commandElement = "getFunctionalGroups"
                    },

                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_EventHandler",
                        commandElement = "getSupportedEventList"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_EventHandler",
                        commandElement = "getEventSubList"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_EventHandler",
                        commandElement = "setEventSub"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_EventHandler",
                        commandElement = "clearEventSub"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_EventHandler",
                        commandElement = "logContentEvent"
                    },

                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Cabinet",
                        commandElement = "getCallAttendantState"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Cabinet",
                        commandElement = "setCallAttendantState"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Cabinet",
                        commandElement = "getDeviceVisibleState"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Cabinet",
                        commandElement = "setDeviceVisibleState"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Cabinet",
                        commandElement = "getCardState"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Cabinet",
                        commandElement = "contentToHostMessage"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Cabinet",
                        commandElement = "getCabinetStatus"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Cabinet",
                        commandElement = "setCardRemoved"
                    },

                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Meters",
                        commandElement = "getSupportedMeterList"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Meters",
                        commandElement = "getMeterSub"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Meters",
                        commandElement = "setMeterSub"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Meters",
                        commandElement = "clearMeterSub"
                    },
                    new c_mediaDisplayProfile.localCommandList.localCommandItem
                    {
                        funtionalGroup = "IGT_Meters",
                        commandElement = "getMeterInfo"
                    }
                }
            };
        }

        /// <summary>
        /// Create list of implemented <see cref="c_mediaDisplayProfile.localEventList.localEventItem"/> events
        /// </summary>
        /// <returns>List as <see cref="c_mediaDisplayProfile.localEventList"/></returns>
        public static c_mediaDisplayProfile.localEventList ImplementedEventList()
        {
            return new c_mediaDisplayProfile.localEventList()
            {
                localEventItem1 = new[]
                {
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME101",
                        eventText = @"Game Idle"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME102",
                        eventText = @"Primary Game Escrow"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME103",
                        eventText = @"Primary Game Started"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME104",
                        eventText = @"Primary Game Ended"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME105",
                        eventText = @"Progressive Pending"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME106",
                        eventText = @"Secondary Game Choice"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME107",
                        eventText = @"Secondary Game Escrow"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME108",
                        eventText = @"Secondary Game Started"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME109",
                        eventText = @"Secondary Game Ended"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME110",
                        eventText = @"Pay Game Results"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_GME111",
                        eventText = @"Game Ended"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME101",
                        eventText = @"Game Idle"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME102",
                        eventText = @"Primary Game Escrow"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME103",
                        eventText = @"Primary Game Started"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME104",
                        eventText = @"Primary Game Ended"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME105",
                        eventText = @"Progressive Pending"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME106",
                        eventText = @"Secondary Game Choice"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME107",
                        eventText = @"Secondary Game Escrow"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME108",
                        eventText = @"Secondary Game Started"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME109",
                        eventText = @"Secondary Game Ended"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME110",
                        eventText = @"Pay Game Results"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_GME111",
                        eventText = @"Game Ended"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_BTN100",
                        eventText = @"Call Attendant/Change Button Pressed"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_IDE100",
                        eventText = @"Card Inserted"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_IDE101",
                        eventText = @"Card Removed"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"IGT_IDE102",
                        eventText = @"Card Abandoned"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem { eventCode = @"IGT_CBE100" },
                    new c_mediaDisplayProfile.localEventList.localEventItem { eventCode = @"G2S_CBE101" },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_MDE100",
                        eventText = @"EGM Local Media Display Interface Open"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_MDE101",
                        eventText = @"EGM Local Media Display Interface Closed"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_MDE001",
                        eventText = @"Media Display Device Shown"
                    },
                    new c_mediaDisplayProfile.localEventList.localEventItem
                    {
                        eventCode = @"G2S_MDE002",
                        eventText = @"Media Display Device Hidden"
                    }
                }
            };
        }
    }
}
