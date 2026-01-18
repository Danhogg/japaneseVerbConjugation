using Avalonia.Controls;
using JapaneseVerbConjugation.AvaloniaUI.ViewModels;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace JapaneseVerbConjugation.AvaloniaUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(this);
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        VerbStoreStore.CreateBackup();
    }

    private void OnNotesTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.NotifyNotesEdited();
        }
    }
}