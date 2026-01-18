using Avalonia.Controls;
using JapaneseVerbConjugation.AvaloniaUI.ViewModels;

namespace JapaneseVerbConjugation.AvaloniaUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(this);
    }

    private void OnNotesTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.NotifyNotesEdited();
        }
    }
}