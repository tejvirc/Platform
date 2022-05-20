namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using Contracts.OperatorMenu;
    using System.Collections.Generic;

    public class DialogButtonCustomText : List<IDialogButtonCustomTextItem>
    {
        public void Add(DialogButton button, string text)
        {
            var item = new DialogButtonCustomTextItem(button, text);
            Add(item);
        }
    }

    public class DialogButtonCustomTextItem : IDialogButtonCustomTextItem
    {
        public DialogButtonCustomTextItem(DialogButton button, string text)
        {
            Button = button;
            Text = text;
        }

        public DialogButton Button { get; set; }
        public string Text { get; set; }
    }
}
