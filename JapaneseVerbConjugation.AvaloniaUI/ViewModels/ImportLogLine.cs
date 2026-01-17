using Avalonia.Media;

namespace JapaneseVerbConjugation.AvaloniaUI.ViewModels
{
    public sealed class ImportLogLine
    {
        public ImportLogLine(string message, IBrush colour)
        {
            Message = message;
            Colour = colour;
        }

        public string Message { get; }
        public IBrush Colour { get; }
    }
}
