namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Windows.Input;

    /// <summary>
    ///     Interface to provide data for message overlay
    /// </summary>
    public interface IMessageOverlayData
    {
        /// <summary>
        ///     Primary message text
        /// </summary>
        string Text { get; set; }

        /// <summary>
        ///     Secondary message text
        /// </summary>
        string SubText { get; set; }

        /// <summary>
        ///     Additional secondary message text
        /// </summary>
        string SubText2 { get; set; }

        /// <summary>
        ///     Extra additional secondary message text
        /// </summary>
        string SubText3 { get; set; }

        /// <summary>
        ///     Replay Text
        /// </summary>
        string ReplayText { get; set; }

        /// <summary>
        ///     Name of the image resource to be used
        /// </summary>
        string DisplayImageResourceKey { get; set; }

        /// <summary>
        ///     Whether SubText is visible or not
        /// </summary>
        bool IsSubTextVisible { get; set; }

        /// <summary>
        ///     Whether SubText2 is visible or not
        /// </summary>
        bool IsSubText2Visible { get; set; }

        /// <summary>
        ///     Whether SubText3 is visible or not
        /// </summary>
        bool IsSubText3Visible { get; set; }

        /// <summary>
        ///     Whether data should be displayed for events
        /// </summary>
        bool DisplayForEvents { get; set; }

        /// <summary>
        ///     Whether the game is handling presentation of a handpay
        /// </summary>
        bool GameHandlesHandPayPresentation { get; set; }

        /// <summary>
        ///     Text to appear on buttons
        /// </summary>
        string ButtonText { get; set; }

        /// <summary>
        ///     Whether a button should be visible or not
        /// </summary>
        bool IsButtonVisible { get; set; }

        /// <summary>
        ///     The command to send when a button is pressed
        /// </summary>
        ICommand ButtonCommand { get; set; }

        /// <summary>
        ///     Whether to display info for popups
        /// </summary>
        bool DisplayForPopUp { get; set; }

        /// <summary>
        ///     If a dialog box is visible
        /// </summary>
        bool IsDialogVisible { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool IsCashOutDialogVisible { get; set; }
        /// <summary>
        ///     Whether dialog is fading out or not
        /// </summary>
        bool IsDialogFadingOut { get; set; }

        /// <summary>
        ///     Opacity of element
        /// </summary>
        double Opacity { get; set; }

        // <summary>
        ///     Opacity used for the black background, if GameHandlesHandPayPresentation is true then this will be 0
        /// </summary>
        double FinalOpacity { get; }

        /// <summary>
        ///     Whether scaling needs to occur or not
        /// </summary>
        bool IsScalingNeeded { get; }

        /// <summary>
        ///     Log Text Generation method
        /// </summary>
        /// <returns>a string containing the generated log text</returns>
        string GenerateLogText();

        /// <summary>
        ///     Clears all data
        /// </summary>
        void Clear();
    }
}
