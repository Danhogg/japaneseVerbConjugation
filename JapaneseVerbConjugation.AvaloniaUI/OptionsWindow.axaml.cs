using Avalonia.Controls;
using JapaneseVerbConjugation.AvaloniaUI.ViewModels;
using JapaneseVerbConjugation.Models;

namespace JapaneseVerbConjugation.AvaloniaUI
{
    public partial class OptionsWindow : Window
    {
        public AppOptions Result { get; private set; }

        public OptionsWindow()
        {
            InitializeComponent();
            Result = new AppOptions();
        }

        public OptionsWindow(AppOptions current) : this()
        {
            DataContext = new OptionsViewModel(current);
            Result = current;
        }

        private void OnSaveClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is OptionsViewModel vm)
            {
                Result = vm.BuildResult();
            }
            Close(true);
        }

        private void OnCancelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => Close(false);
    }
}
