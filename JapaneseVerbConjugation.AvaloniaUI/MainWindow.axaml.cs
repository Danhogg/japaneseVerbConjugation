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
}