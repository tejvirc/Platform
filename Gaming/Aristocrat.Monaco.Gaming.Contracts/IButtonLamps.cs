// ReSharper disable UnusedMember.Global
using System.Collections.Generic;

namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary> LampState </summary>
    public enum LampState
    {
        /// <summary> Off </summary>
        Off = 0,

        /// <summary> On </summary>
        On,

        /// <summary> Blink </summary>
        Blink
    }

    /// <summary> Common names for buttons mapped to a buttonId </summary>
    public enum LampName
    {
        /// <summary> Bash (play/hit/etc) </summary>
        Bash = 13,
        /// <summary> Collect </summary>
        Collect = 1,
        /// <summary> Bet 1 </summary>
        Bet1 = 8,
        /// <summary> Bet 2 </summary>
        Bet2 = 9,
        /// <summary> Bet 3 </summary>
        Bet3 = 10,
        /// <summary> Bet 4 </summary>
        Bet4 = 11,
        /// <summary> Bet 5 </summary>
        Bet5 = 12,
        /// <summary> TakeWin </summary>
        TakeWin = 14,
        /// <summary> MaxBet </summary>
        MaxBet = 15,
        /// <summary> Playline 1 </summary>
        Playline1 = 2,
        /// <summary> Playline 2 </summary>
        Playline2 = 3,
        /// <summary> Playline 3 </summary>
        Playline3 = 4,
        /// <summary> Playline 4 </summary>
        Playline4 = 5,
        /// <summary> Playline 5 </summary>
        Playline5 = 6,
        /// <summary> Service </summary>
        Service = 7
    }

    /// <summary>
    ///     Interface for controlling button lamps.
    /// </summary>
    public interface IButtonLamps
    {
        /// <summary>
        ///     Gets the lamp state.
        /// </summary>
        /// <param name="buttonId">The button Id.</param>
        /// <returns>The lamp state.</returns>
        LampState GetLampState(int buttonId);

        /// <summary>
        ///     Sets the lamp state.
        /// </summary>
        /// <param name="buttonId">The button Id.</param>
        /// <param name="state">The lamp state.</param>
        void SetLampState(int buttonId, LampState state);

        /// <summary>
        ///     Sets the lamp state.
        /// </summary>
        /// <param name="buttonsLampState">list of button ids</param>

        void SetLampState(IList<ButtonLampState> buttonsLampState);

        /// <summary>
        ///     Disables all the lamps
        /// </summary>
        void DisableLamps();

        /// <summary>
        ///     Enables all the lamps
        /// </summary>
        void EnableLamps();
    }

    /// <summary> Button and corresponding state of lamp for static button deck </summary>
    public class ButtonLampState
    {
        /// <summary> Id of button</summary>
        public int ButtonId;
        /// <summary> State Of Button</summary>
        public LampState State;

        /// <summary> ButtonState </summary>
        public ButtonLampState(int buttonId, LampState state)
        {
            ButtonId = buttonId;
            State = state;
        }
    }
}