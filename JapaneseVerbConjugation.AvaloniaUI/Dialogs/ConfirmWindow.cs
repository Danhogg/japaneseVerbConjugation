using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace JapaneseVerbConjugation.AvaloniaUI
{
    public sealed class ConfirmWindow : Window
    {
        public ConfirmWindow(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel")
        {
            Title = title;
            Width = 340;
            Height = 170;
            CanResize = false;

            var text = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(16, 16, 16, 8)
            };

            var confirm = new Button
            {
                Content = confirmText,
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(8, 0, 0, 12)
            };
            confirm.Classes.Add("danger");
            confirm.Click += (_, _) => Close(true);

            var cancel = new Button
            {
                Content = cancelText,
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 8, 12)
            };
            cancel.Click += (_, _) => Close(false);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            buttons.Children.Add(cancel);
            buttons.Children.Add(confirm);

            var root = new StackPanel
            {
                Spacing = 8
            };
            root.Children.Add(text);
            root.Children.Add(buttons);

            Content = root;
        }
    }
}
