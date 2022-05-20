namespace Aristocrat.Monaco.G2S.Common.GAT.Validators
{
    using System.IO;
    using FluentValidation.Validators;
    using Kernel.Contracts.Components;

    /// <summary>
    ///     Component path validator
    /// </summary>
    public class ComponentPathValidator : PropertyValidator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentPathValidator" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public ComponentPathValidator(string errorMessage)
            : base(errorMessage)
        {
        }

        /// <summary>
        ///     Returns true if component path is valid.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     <c>true</c> if the specified context is valid; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            var path = context.PropertyValue.ToString();

            try
            {
                var result = Path.GetFullPath(path);

                if (!string.IsNullOrEmpty(result))
                {
                    //// valid path string
                    var component = (Component)context.Instance;
                    var hasExtension = Path.HasExtension(path);
                    if (component.FileSystemType == FileSystemType.File && hasExtension)
                    {
                        return true;
                    }

                    if (!hasExtension)
                    {
                        var info = new DirectoryInfo(path);

                        if (component.FileSystemType == FileSystemType.Directory && info.Parent != null)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
