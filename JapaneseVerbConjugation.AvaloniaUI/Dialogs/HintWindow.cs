using Avalonia.Controls;
using Avalonia.Layout;

namespace JapaneseVerbConjugation.AvaloniaUI
{
    public sealed class HintWindow : Window
    {
        private readonly string _full;
        private readonly TextBlock _label;
        private readonly Button _revealButton;

        public HintWindow(string masked, string full)
        {
            _full = full;

            Title = "Hint";
            Width = 320;
            Height = 180;
            CanResize = false;

            _label = new TextBlock
            {
                Text = masked,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(16),
                FontSize = 22,
                FontWeight = Avalonia.Media.FontWeight.Bold
            };

            _revealButton = new Button
            {
                Content = "Show answer",
                Width = 140,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 12)
            };
            _revealButton.Click += (_, _) => Reveal();

            var root = new StackPanel { Spacing = 8 };
            root.Children.Add(_label);
            root.Children.Add(_revealButton);

            Content = root;
        }

        private void Reveal()
        {
            _label.Text = _full;
            _revealButton.IsEnabled = false;
            _revealButton.Content = "Answer shown";
        }
    }
}
