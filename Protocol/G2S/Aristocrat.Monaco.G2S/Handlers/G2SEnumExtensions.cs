namespace Aristocrat.Monaco.G2S.Handlers
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager.Storage;
    using Data.Model;
    using Hardware.Contracts.IdReader;
    using DeviceClass = Data.Model.DeviceClass;

    /// <summary>
    ///     Extensions to convert G2S string enums to code enums in both directions.
    /// </summary>
    public static class G2SEnumExtensions
    {
        /// <summary>
        ///     Converts ApplyCondition to the G2S value.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>String value of G2S protocol.</returns>
        public static string ToG2SString(this ApplyCondition condition)
        {
            switch (condition)
            {
                case ApplyCondition.Immediate:
                    return "G2S_immediate";
                case ApplyCondition.EgmAction:
                    return "G2S_egmAction";
                case ApplyCondition.Disable:
                    return "G2S_disable";
                case ApplyCondition.Cancel:
                    return "G2S_cancel";
            }

            throw new ArgumentException("Not valid value for ApplyCondition");
        }

        /// <summary>
        ///     Converts DisableCondition to the G2S value.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>String value of G2S protocol.</returns>
        public static string ToG2SString(this DisableCondition condition)
        {
            switch (condition)
            {
                case DisableCondition.None:
                    return "G2S_none";
                case DisableCondition.Idle:
                    return "G2S_idle";
                case DisableCondition.Immediate:
                    return "G2S_immediate";
                case DisableCondition.ZeroCredits:
                    return "G2S_zeroCredits";
            }

            throw new ArgumentException("Not valid value for DisableCondition");
        }

        /// <summary>
        ///     Converts G2S value to the DisableCondition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>String value of G2S protocol.</returns>
        public static DisableCondition DisableConditionFromG2SString(this string condition)
        {
            switch (condition)
            {
                case "G2S_idle":
                    return DisableCondition.Idle;
                case "G2S_immediate":
                    return DisableCondition.Immediate;
                case "G2S_zeroCredits":
                    return DisableCondition.ZeroCredits;
                default:
                    return DisableCondition.None;
            }
        }

        /// <summary>
        ///     Converts G2S value to the ApplyCondition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>String value of G2S protocol.</returns>
        public static ApplyCondition ApplyConditionFromG2SString(this string condition)
        {
            switch (condition)
            {
                case "G2S_immediate":
                    return ApplyCondition.Immediate;
                case "G2S_disable":
                    return ApplyCondition.Disable;
                case "G2S_egmAction":
                    return ApplyCondition.EgmAction;
                case "G2S_cancel":
                    return ApplyCondition.Cancel;
            }

            throw new ArgumentException("Not valid value for ApplyCondition");
        }

        /// <summary>
        ///     Converts ScriptState to G2S value.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Enum script state.</returns>
        public static string G2SScriptStatesFromScriptStates(this ScriptState state)
        {
            switch (state)
            {
                case ScriptState.Canceled:
                    return "G2S_cancelled";
                case ScriptState.Completed:
                    return "G2S_completed";
                case ScriptState.Error:
                    return "G2S_error";
                case ScriptState.InProgress:
                    return "G2S_inProgress";
                case ScriptState.PendingAuthorization:
                    return "G2S_pendingAuthorization";
                case ScriptState.PendingDateTime:
                    return "G2S_pendingDateTime";
                case ScriptState.PendingDisable:
                    return "G2S_pendingDisable";
                case ScriptState.PendingOperatorAction:
                    return "G2S_pendingOperatorAction";
                case ScriptState.PendingPackage:
                    return "G2S_pendingPackage";
            }

            throw new ArgumentException("Not valid value for ScriptState");
        }

        /// <summary>
        ///     Converts G2S value to the TimeoutActionType.
        /// </summary>
        /// <param name="timeoutAction">The timeout action.</param>
        /// <returns>String value of G2S protocol.</returns>
        public static TimeoutActionType TimeoutActionFromG2SString(this string timeoutAction)
        {
            switch (timeoutAction)
            {
                case "G2S_abort":
                    return TimeoutActionType.Abort;
                case "G2S_ignore":
                    return TimeoutActionType.Ignore;
            }

            throw new ArgumentException("Not valid value for TimeoutActionType");
        }

        /// <summary>
        ///     Devices the class to g2 s string.
        /// </summary>
        /// <param name="deviceClass">The device class.</param>
        /// <returns>G2S device class</returns>
        /// <exception cref="System.ArgumentException">Not valid value for G2S deviceClass</exception>
        public static string DeviceClassToG2SString(this DeviceClass deviceClass)
        {
            switch (deviceClass)
            {
                case DeviceClass.Communications:
                    return "G2S_communications";
                case DeviceClass.Cabinet:
                    return "G2S_cabinet";
                case DeviceClass.EventHandler:
                    return "G2S_eventHandler";
                case DeviceClass.Meters:
                    return "G2S_meters";
                case DeviceClass.GamePlay:
                    return "G2S_gamePlay";
                case DeviceClass.DeviceConfig:
                    return "G2S_deviceConfig";
                case DeviceClass.CommConfig:
                    return "G2S_commConfig";
                case DeviceClass.OptionConfig:
                    return "G2S_optionConfig";
                case DeviceClass.Download:
                    return "G2S_download";
                case DeviceClass.HandPay:
                    return "G2S_handpay";
                case DeviceClass.CoinAcceptor:
                    return "G2S_coinAcceptor";
                case DeviceClass.NoteAcceptor:
                    return "G2S_noteAcceptor";
                case DeviceClass.Hopper:
                    return "G2S_hopper";
                case DeviceClass.NoteDispenser:
                    return "G2S_noteDispenser";
                case DeviceClass.Printer:
                    return "G2S_printer";
                case DeviceClass.Progressive:
                    return "G2S_progressive";
                case DeviceClass.IdReader:
                    return "G2S_idReader";
                case DeviceClass.Bonus:
                    return "G2S_bonus";
                case DeviceClass.Player:
                    return "G2S_player";
                case DeviceClass.Voucher:
                    return "G2S_voucher";
                case DeviceClass.Wat:
                    return "G2S_wat";
                case DeviceClass.Gat:
                    return "G2S_gat";
                case DeviceClass.Central:
                    return "G2S_central";
                case DeviceClass.All:
                    return "G2S_all";
                case DeviceClass.Dft:
                    return "G2S_dft";
                case DeviceClass.Employee:
                    return "G2S_employee";
                case DeviceClass.GameTheme:
                    return "G2S_gameTheme";
                case DeviceClass.Hardware:
                    return "G2S_hardware";
                case DeviceClass.InformedPlayer:
                    return "G2S_informedPlayer";
                case DeviceClass.Tournament:
                    return "G2S_tournament";
                case DeviceClass.Spc:
                    return "G2S_spc";
                case DeviceClass.CashOut:
                    return "GTK_cashout";
                case DeviceClass.Storage:
                    return "GTK_storage";
                case DeviceClass.MediaDisplay:
                    return "IGT_mediaDisplay";
                case DeviceClass.SmartCard:
                    return "IGT_smartCard";
                case DeviceClass.Chooser:
                    return "G2S_chooser";
                default:
                    throw new ArgumentException("Not valid value for G2S deviceClass");
            }
        }

        /// <summary>
        ///     Converts G2S value to the DeviceClass
        /// </summary>
        /// <param name="deviceClass">The device class.</param>
        /// <returns>DeviceClass enum value from deviceClass string.</returns>
        public static DeviceClass DeviceClassFromG2SString(this string deviceClass)
        {
            switch (deviceClass)
            {
                case "G2S_communications":
                    return DeviceClass.Communications;
                case "G2S_cabinet":
                    return DeviceClass.Cabinet;
                case "G2S_eventHandler":
                    return DeviceClass.EventHandler;
                case "G2S_meters":
                    return DeviceClass.Meters;
                case "G2S_gamePlay":
                    return DeviceClass.GamePlay;
                case "G2S_deviceConfig":
                    return DeviceClass.DeviceConfig;
                case "G2S_commConfig":
                    return DeviceClass.CommConfig;
                case "G2S_optionConfig":
                    return DeviceClass.OptionConfig;
                case "G2S_download":
                    return DeviceClass.Download;
                case "G2S_handpay":
                    return DeviceClass.HandPay;
                case "G2S_coinAcceptor":
                    return DeviceClass.CoinAcceptor;
                case "G2S_noteAcceptor":
                    return DeviceClass.NoteAcceptor;
                case "G2S_hopper":
                    return DeviceClass.Hopper;
                case "G2S_noteDispenser":
                    return DeviceClass.NoteDispenser;
                case "G2S_printer":
                    return DeviceClass.Printer;
                case "G2S_progressive":
                    return DeviceClass.Progressive;
                case "G2S_idReader":
                    return DeviceClass.IdReader;
                case "G2S_bonus":
                    return DeviceClass.Bonus;
                case "G2S_player":
                    return DeviceClass.Player;
                case "G2S_voucher":
                    return DeviceClass.Voucher;
                case "G2S_wat":
                    return DeviceClass.Wat;
                case "G2S_gat":
                    return DeviceClass.Gat;
                case "G2S_central":
                    return DeviceClass.Central;
                case "G2S_all":
                    return DeviceClass.All;
                case "G2S_dft":
                    return DeviceClass.Dft;
                case "G2S_employee":
                    return DeviceClass.Employee;
                case "G2S_gameTheme":
                    return DeviceClass.GameTheme;
                case "G2S_hardware":
                    return DeviceClass.Hardware;
                case "G2S_informedPlayer":
                    return DeviceClass.InformedPlayer;
                case "G2S_tournament":
                    return DeviceClass.Tournament;
                case "GTK_cashout":
                    return DeviceClass.CashOut;
                case "GTK_storage":
                    return DeviceClass.Storage;
                case "IGT_mediaDisplay":
                    return DeviceClass.MediaDisplay;
                case "IGT_smartCard":
                    return DeviceClass.SmartCard;
                case "G2S_chooser":
                    return DeviceClass.Chooser;
                case "G2S_auditMeters":
                    return DeviceClass.AuditMeters;
                case "G2S_spc":
                    return DeviceClass.Spc;
                default:
                    throw new ArgumentException("Not valid value for DeviceClass");
            }
        }

        /// <summary>
        ///     Converts the IdReaderTypes to the G2S equivalent
        /// </summary>
        /// <param name="this">An IdReaderTypes enum</param>
        /// <returns>the G2S equivalent</returns>
        public static string ToIdReaderType(this IdReaderTypes @this)
        {
            switch (@this)
            {
                case IdReaderTypes.MagneticCard:
                    return IdReaderType.MagneticCard;
                case IdReaderTypes.ProximityCard:
                    return IdReaderType.ProximityCard;
                case IdReaderTypes.Fingerprint:
                    return IdReaderType.FingerScan;
                case IdReaderTypes.Retina:
                    return IdReaderType.RetinaScanner;
                case IdReaderTypes.SmartCard:
                    return IdReaderType.Smartcard;
                case IdReaderTypes.BarCode:
                    return IdReaderType.Barcode;
                case IdReaderTypes.KeyPad:
                    return IdReaderType.Keypad;
                case IdReaderTypes.Hollerith:
                    return IdReaderType.HollerithPunchCard;
                case IdReaderTypes.None:
                    return IdReaderType.None;
                case IdReaderTypes.Facial:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        ///     Converts the G2S <see cref="t_creditTypes" /> to an <see cref="AccountType" />
        /// </summary>
        /// <param name="this">The credit type</param>
        /// <returns>The account type</returns>
        public static AccountType ToAccountType(this t_creditTypes @this)
        {
            switch (@this)
            {
                case t_creditTypes.G2S_cashable:
                    return AccountType.Cashable;
                case t_creditTypes.G2S_promo:
                    return AccountType.Promo;
                case t_creditTypes.G2S_nonCash:
                    return AccountType.NonCash;
                default:
                    return AccountType.Cashable;
            }
        }

        /// <summary>
        ///     Converts an <see cref="AccountType" /> to a G2S <see cref="t_creditTypes" />
        /// </summary>
        /// <param name="this">The account type</param>
        /// <returns>The credit type</returns>
        public static t_creditTypes ToCreditType(this AccountType @this)
        {
            switch (@this)
            {
                case AccountType.Cashable:
                    return t_creditTypes.G2S_cashable;
                case AccountType.Promo:
                    return t_creditTypes.G2S_promo;
                case AccountType.NonCash:
                    return t_creditTypes.G2S_nonCash;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        ///     Converts an <see cref="IdReaderTypes" /> to the G2S type
        /// </summary>
        /// <param name="this">The Id Reader Type</param>
        /// <returns>The G2S type</returns>
        public static string ToReaderType(this IdReaderTypes @this)
        {
            switch (@this)
            {
                case IdReaderTypes.MagneticCard:
                    return @"G2S_magCard";
                case IdReaderTypes.ProximityCard:
                    return @"G2S_proxCard";
                case IdReaderTypes.Fingerprint:
                    return @"G2S_fingerScan";
                case IdReaderTypes.Retina:
                    return @"G2S_retinaScan";
                case IdReaderTypes.SmartCard:
                    return @"G2S_smartCard";
                case IdReaderTypes.BarCode:
                    return @"G2S_barCode";
                case IdReaderTypes.KeyPad:
                    return @"G2S_keyPad";
                case IdReaderTypes.Hollerith:
                    return @"G2Shollerith";
                case IdReaderTypes.Facial:
                    return @"G2S_facialScan";
                case IdReaderTypes.None:
                    return @"G2S_none";
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        ///     Converts the G2S type to an <see cref="IdReaderTypes" />
        /// </summary>
        /// <param name="this">The G2S Id Reader Type</param>
        /// <returns>The Id Reader Type</returns>
        public static IdReaderTypes ToReaderType(this string @this)
        {
            switch (@this)
            {
                case @"G2S_magCard":
                    return IdReaderTypes.MagneticCard;
                case @"G2S_proxCard":
                    return IdReaderTypes.ProximityCard;
                case @"G2S_fingerScan":
                    return IdReaderTypes.Fingerprint;
                case @"G2S_retinaScan":
                    return IdReaderTypes.Retina;
                case @"G2S_smartCard":
                    return IdReaderTypes.SmartCard;
                case @"G2S_barCode":
                    return IdReaderTypes.BarCode;
                case @"G2S_keyPad":
                    return IdReaderTypes.KeyPad;
                case @"G2Shollerith":
                    return IdReaderTypes.Hollerith;
                case @"G2S_facialScan":
                    return IdReaderTypes.Facial;
                case @"G2S_none":
                    return IdReaderTypes.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }
    }
}