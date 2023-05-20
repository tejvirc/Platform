namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.Text;
    using Contracts.Process;

    /// <summary>
    ///     Initialization data for the game process
    /// </summary>
    public class GameProcessArgs : IProcessArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameProcessArgs" /> class.
        /// </summary>
        /// <param name="variation">The variation</param>
        /// <param name="denomination">The requested denomination</param>
        /// <param name="gameBottomHwnd">Game bottom window handle</param>
        /// <param name="gameTopHwnd">Game top window handle</param>
        /// <param name="gameVirtualButtonDeckHwnd">Game virtual button deck window handle</param>
        /// <param name="gameTopperHwnd">Game topper window handle</param>
        /// <param name="gameDllPath">The path to the game DLL so the host can launch the game.</param>
        /// <param name="logPath">The path for the host and game logs.</param>
        /// <param name="jurisdiction">The jurisdiction to be applied to the game.</param>
        /// <param name="centralDeterminant">true if this is a central determinant game</param>
        /// <param name="fpsLock">The frame rate to lock the game into</param>
        /// <param name="args">Additional runtime args</param>
        public GameProcessArgs(
            string variation,
            long denomination,
            IntPtr gameBottomHwnd,
            IntPtr gameTopHwnd,
            IntPtr gameVirtualButtonDeckHwnd,
            IntPtr gameTopperHwnd,
            string gameDllPath,
            string logPath,
            string jurisdiction,
            bool centralDeterminant,
            int fpsLock,
            string args)
        {
            Variation = variation;
            Denomination = denomination;
            GameBottomHwnd = gameBottomHwnd;
            GameTopHwnd = gameTopHwnd;
            GameVirtualButtonDeckHwnd = gameVirtualButtonDeckHwnd;
            GameTopperHwnd = gameTopperHwnd;
            GameDllPath = gameDllPath;
            LogPath = logPath;
            Jurisdiction = jurisdiction;
            CentralDeterminant = centralDeterminant;
            FpsLock = fpsLock;
            Args = args;
        }

        /// <summary>
        ///     Gets the variation
        /// </summary>
        public string Variation { get; }

        /// <summary>
        ///     Gets the enabled game denomination
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        ///     Gets the game bottom window
        /// </summary>
        public IntPtr GameBottomHwnd { get; }

        /// <summary>
        ///     Gets the game top window
        /// </summary>
        public IntPtr GameTopHwnd { get; }

        /// <summary>
        ///     Gets the game virtual button deck window
        /// </summary>
        public IntPtr GameVirtualButtonDeckHwnd { get; }

        /// <summary>
        ///     Gets the game topper window
        /// </summary>
        public IntPtr GameTopperHwnd { get; }

        /// <summary>
        ///     Gets the game DLL path
        /// </summary>
        public string GameDllPath { get; }

        /// <summary>
        ///     Gets the log path
        /// </summary>
        public string LogPath { get; }

        /// <summary>
        ///     Gets the jurisdiction
        /// </summary>
        public string Jurisdiction { get; }

        /// <summary>
        ///     Gets a value indicating whether or not this is central determinant game
        /// </summary>
        public bool CentralDeterminant { get; }

        /// <summary>
        ///     Gets a value for the frame rate lock
        /// </summary>
        public int FpsLock { get; }

        /// <summary>
        ///     Gets the additional runtime args
        /// </summary>
        public string Args { get; }

        /// <inheritdoc />
        public string Build()
        {
            var str = new StringBuilder();
            str.Append($"--display0 {GameBottomHwnd} ");

            if (GameTopHwnd != IntPtr.Zero)
            {
                str.Append($"--display1 {GameTopHwnd} ");
            }

            if (GameVirtualButtonDeckHwnd != IntPtr.Zero)
            {
                str.Append($"--display2 {GameVirtualButtonDeckHwnd} ");
            }

            if (GameTopperHwnd != IntPtr.Zero)
            {
                str.Append($"--display3 {GameTopperHwnd} ");
            }

            if (FpsLock > 0)
            {
                str.Append($"--fps {FpsLock} ");
            }

            str.Append($"--game \"{GameDllPath}\" ");
            str.Append("--wcf ");
            str.Append("--windowmode direct ");
            str.Append($"--log \"{LogPath}\" ");
            str.Append($"--variation {Variation} ");
            str.Append($"--denomination {Denomination} ");
            str.Append($"--jurisdiction \"{Jurisdiction}\" ");
            str.Append($"{Args} ");

            if (CentralDeterminant)
            {
                str.Append("--cds ");
            }

            return str.ToString();
        }
    }
}
