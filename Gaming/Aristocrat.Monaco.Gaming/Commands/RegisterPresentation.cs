namespace Aristocrat.Monaco.Gaming.Commands
{
    using Contracts;

    public class RegisterPresentation
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RegisterPresentation" /> class.
        /// </summary>
        /// <param name="result">If the game is registering</param>
        /// <param name="typeData">The desired presentation types that will be handled by the games</param>
        public RegisterPresentation(bool result, params PresentationOverrideTypes[] typeData)
        {
            Result = result;
            TypeData = typeData;
        }

        /// <summary>
        ///     Gets the desired presentation types
        /// </summary>
        public PresentationOverrideTypes[] TypeData { get; }

        /// <summary>
        ///     Gets a value indicating if the presentations were registered successfully
        /// </summary>
        public bool Result { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the presentation override was registered
        /// </summary>
        public bool Success { get; set; }
    }
}
