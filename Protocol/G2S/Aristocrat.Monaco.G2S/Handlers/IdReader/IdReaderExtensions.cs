namespace Aristocrat.Monaco.G2S.Handlers.IdReader
{
    using System;
    using Aristocrat.G2S.Protocol.v21;
    using Hardware.Contracts.IdReader;

    public static class IdReaderExtensions
    {
        public static string ToIdType(this IdTypes @this)
        {
            switch (@this)
            {
                case IdTypes.Player:
                    return @"G2S_player";
                case IdTypes.Employee:
                    return @"G2S_employee";
                case IdTypes.Regulator:
                    return @"G2S_regulator";
                case IdTypes.Vendor:
                    return @"G2S_vendor";
                case IdTypes.Anonymous:
                    return @"G2S_anonymous";
                case IdTypes.Invalid:
                    return @"G2S_invalid";
                case IdTypes.Unknown:
                    return @"G2S_unknown";
                case IdTypes.None:
                    return @"G2S_none";
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static IdTypes ToIdType(this string @this)
        {
            switch (@this)
            {
                case @"G2S_player":
                    return IdTypes.Player;
                case @"G2S_employee":
                    return IdTypes.Employee;
                case @"G2S_regulator":
                    return IdTypes.Regulator;
                case @"G2S_vendor":
                    return IdTypes.Vendor;
                case @"G2S_anonymous":
                    return IdTypes.Anonymous;
                case @"G2S_invalid":
                    return IdTypes.Invalid;
                case @"G2S_unknown":
                    return IdTypes.Unknown;
                case @"G2S_none":
                    return IdTypes.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static int ToTrackId(this IdReaderTracks @this)
        {
            if ((@this & IdReaderTracks.Track1) == IdReaderTracks.Track1)
            {
                return 1;
            }

            if ((@this & IdReaderTracks.Track2) == IdReaderTracks.Track2)
            {
                return 2;
            }

            if ((@this & IdReaderTracks.Track3) == IdReaderTracks.Track3)
            {
                return 3;
            }

            return (@this & IdReaderTracks.Icc) == IdReaderTracks.Icc ? 4 : 1; // Default is 1 per the protocol
        }

        public static IdReaderTracks ToTrackId(this int @this)
        {
            switch (@this)
            {
                case 0:
                    return IdReaderTracks.None;
                case 1:
                    return IdReaderTracks.Track1;
                case 2:
                    return IdReaderTracks.Track2;
                case 3:
                    return IdReaderTracks.Track3;
                case 4:
                    return IdReaderTracks.Icc;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static t_idValidMethods ToValidationMethod(this IdValidationMethods @this)
        {
            switch (@this)
            {
                case IdValidationMethods.Host:
                    return t_idValidMethods.G2S_host;
                case IdValidationMethods.Self:
                    return t_idValidMethods.G2S_self;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static IdValidationMethods ToValidationMethod(this t_idValidMethods @this)
        {
            switch (@this)
            {
                case t_idValidMethods.G2S_host:
                    return IdValidationMethods.Host;
                case t_idValidMethods.G2S_self:
                    return IdValidationMethods.Self;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static Identity ToIdentity(this setIdValidation @this)
        {
            if (@this == null)
            {
                return null;
            }

            return new Identity
            {
                Age = @this.idAge,
                Classification = @this.idClass,
                DisplayMessages = @this.idDisplayMessages,
                FullName = @this.idFullName,
                Gender = (IdGenders)@this.idGender,
                IsAnniversary = @this.idAnniversary,
                IsBanned = @this.idBanned,
                IsBirthday = @this.idBirthday,
                IsVip = @this.idVIP,
                LocaleId = @this.localeId,
                Number = @this.idNumber,
                PlayerId = @this.playerId,
                PlayerRank = @this.idRank,
                PreferredName = @this.idPreferName,
                PrivacyRequested = @this.idPrivacy,
                State = (IdStates)@this.idState,
                Type = @this.idType.ToIdType(),
                ValidationExpired = @this.idValidExpired,
                ValidationSource = (IdValidationSources)@this.idValidSource,
                ValidationTime = @this.idValidDateTime
            };
        }
    }
}