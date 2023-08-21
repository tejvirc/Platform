namespace Aristocrat.Mgam.Client.Command
{
    using System.ComponentModel;

    /// <summary>
    ///     GUI control types for editing or displaying attributes.
    /// </summary>
    public enum CommandControlType
    {
        /// <summary>A text box for strings or numbers.</summary>
        [Description("editbox")]
        EditBox,

        /// <summary>A slider or scroll bar for numbers.</summary>
        [Description("slider")]
        Slider,

        /// <summary>A checkbox for Booleans.</summary>
        [Description("checkbox")]
        CheckBox,

        /// <summary>A group of multiple checkbox controls.</summary>
        [Description("checkboxgroup")]
        CheckBoxGroup,

        /// <summary>A widget for choosing allowed values.</summary>
        [Description("combobox")]
        ComboBox,

        /// <summary>For issuing commands with no parameter.</summary>
        [Description("button")]
        Button
    }
}
