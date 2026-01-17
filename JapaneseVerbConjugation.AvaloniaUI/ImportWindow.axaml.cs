using Avalonia.Controls;
using JapaneseVerbConjugation.AvaloniaUI.ViewModels;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace JapaneseVerbConjugation.AvaloniaUI
{
    public partial class ImportWindow : Window
    {
        private ImportViewModel _vm = null!;
        public bool DidImport { get; private set; }

        public ImportWindow()
        {
            InitializeComponent();
            SetViewModel(new ImportViewModel(VerbStudySession.LoadFromStorage()));
        }

        public ImportWindow(VerbStudySession session) : this()
        {
            SetViewModel(new ImportViewModel(session));
        }

        private async void OnImportClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            DidImport = await _vm.RunImportAsync();
        }

        private void OnCloseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => Close(DidImport);

        private void OnOpenCustomClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => _vm.OpenCustomFile();

        private void OnOpenFolderClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => _vm.OpenCustomFolder();

        private void SetViewModel(ImportViewModel vm)
        {
            _vm = vm;
            DataContext = _vm;
        }
    }
}
