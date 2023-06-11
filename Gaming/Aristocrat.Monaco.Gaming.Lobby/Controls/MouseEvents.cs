namespace Aristocrat.Monaco.Gaming.Lobby.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    public class MouseEvents
    {
        public static readonly DependencyProperty MouseDownProperty =
            DependencyProperty.RegisterAttached(
                "MouseDown",
                typeof(ICommand),
                typeof(MouseEvents),
                new FrameworkPropertyMetadata(MouseDownCallBack));

        public static readonly DependencyProperty PreviewMouseDownProperty =
            DependencyProperty.RegisterAttached(
                "PreviewMouseDown",
                typeof(ICommand),
                typeof(MouseEvents),
                new FrameworkPropertyMetadata(PreviewMouseDownCallBack));

        public static readonly DependencyProperty MouseEventParameterProperty =
            DependencyProperty.RegisterAttached(
                "MouseEventParameter",
                typeof(object),
                typeof(MouseEvents),
                new FrameworkPropertyMetadata(default, null));

        public static void SetMouseDown(DependencyObject sender, ICommand value)
        {
            sender.SetValue(MouseDownProperty, value);
        }

        public static ICommand? GetMouseDown(DependencyObject sender)
        {
            return sender.GetValue(MouseDownProperty) as ICommand;
        }

        public static void SetPreviewMouseDown(DependencyObject sender, ICommand value)
        {
            sender.SetValue(PreviewMouseDownProperty, value);
        }

        public static ICommand? GetPreviewMouseDown(DependencyObject sender)
        {
            return sender.GetValue(PreviewMouseDownProperty) as ICommand;
        }

        public static object GetMouseEventParameter(DependencyObject d)
        {
            return d.GetValue(MouseEventParameterProperty);
        }

        public static void SetMouseEventParameter(DependencyObject d, object value)
        {
            d.SetValue(MouseEventParameterProperty, value);
        }

        private static void MouseDownCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement element)
            {
                if (e.OldValue != null)
                {
                    element.RemoveHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(Handler));
                }

                if (e.NewValue != null)
                {
                    element.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(Handler), true);
                }
            }
        }

        private static void PreviewMouseDownCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement element)
            {
                if (e.OldValue != null)
                {
                    element.RemoveHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(Handler));
                }

                if (e.NewValue != null)
                {
                    element.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(Handler), true);
                }
            }
        }

        private static void Handler(object sender, EventArgs e)
        {
            if (sender is not UIElement element)
            {
                return;
            }

            var cmd = element.GetValue(MouseDownProperty) as ICommand ?? element.GetValue(PreviewMouseDownProperty) as ICommand;
            if (cmd == null)
            {
                return;
            }

            var parameter = element.GetValue(MouseEventParameterProperty) ?? e;

            if (cmd is RoutedCommand routedCmd)
            {
                if (routedCmd.CanExecute(parameter, element))
                {
                    routedCmd.Execute(parameter, element);
                }
            }
            else
            {
                if (cmd.CanExecute(parameter))
                {
                    cmd.Execute(parameter);
                }
            }
        }
    }
}
