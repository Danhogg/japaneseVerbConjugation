using Avalonia.Controls;
using Avalonia.Layout;

namespace JapaneseVerbConjugation.AvaloniaUI
{
    public sealed class MessageWindow : Window
    {
        public MessageWindow(string title, string message)
        {
            Title = title;
            Width = 360;
            Height = 180;
            CanResize = false;

            var text = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(16)
            };

            var ok = new Button
            {
                Content = "OK",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 12)
            };
            ok.Click += (_, _) => Close();

            var root = new StackPanel
            {
                Spacing = 8
            };
            root.Children.Add(text);
            root.Children.Add(ok);

            Content = root;
        }
    }
}
