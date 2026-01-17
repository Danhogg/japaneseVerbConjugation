using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.SharedResources.Methods;

namespace JapaneseVerbConjugation.AvaloniaUI.ViewModels
{
    public sealed class ConjugationOptionViewModel : ViewModelBase
    {
        private bool _isEnabled;

        public ConjugationFormEnum Form { get; }
        public string Label { get; }

        public ConjugationOptionViewModel(ConjugationFormEnum form, bool isEnabled)
        {
            Form = form;
            Label = form.ToDisplayLabel();
            _isEnabled = isEnabled;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }
}
