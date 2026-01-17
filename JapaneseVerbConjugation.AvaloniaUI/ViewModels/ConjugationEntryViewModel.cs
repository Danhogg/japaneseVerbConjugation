using Avalonia.Media;
using JapaneseVerbConjugation.AvaloniaUI.Constants;
using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Methods;

namespace JapaneseVerbConjugation.AvaloniaUI.ViewModels
{
    public sealed class ConjugationEntryViewModel : ViewModelBase
    {
        private string _userInput = string.Empty;
        private ConjugationResultEnum _result = ConjugationResultEnum.Unchecked;

        public ConjugationEntryState State { get; }
        public string Label { get; }

        public ConjugationEntryViewModel(ConjugationEntryState state)
        {
            State = state;
            Label = state.ConjugationForm.ToDisplayLabel();
            RefreshFromState();
        }

        public string UserInput
        {
            get => _userInput;
            set
            {
                if (SetProperty(ref _userInput, value))
                {
                    State.UserInput = value;
                }
            }
        }

        public ConjugationResultEnum Result
        {
            get => _result;
            set
            {
                if (SetProperty(ref _result, value))
                {
                    OnPropertyChanged(nameof(ResultSymbol));
                    OnPropertyChanged(nameof(ResultBrush));
                    OnPropertyChanged(nameof(ResultBackground));
                    OnPropertyChanged(nameof(IsCheckEnabled));
                    OnPropertyChanged(nameof(IsHintEnabled));
                }
            }
        }

        public string ResultSymbol => Result switch
        {
            ConjugationResultEnum.Correct => "✓",
            ConjugationResultEnum.Incorrect => "✗",
            ConjugationResultEnum.Close => "~",
            _ => string.Empty
        };

        public IBrush ResultBrush => Result switch
        {
            ConjugationResultEnum.Correct => UiBrushes.LabelCorrect,
            ConjugationResultEnum.Incorrect => UiBrushes.LabelIncorrect,
            ConjugationResultEnum.Close => UiBrushes.LabelClose,
            _ => Brushes.Transparent
        };

        public IBrush ResultBackground => Result switch
        {
            ConjugationResultEnum.Correct => UiBrushes.TextBoxCorrect,
            ConjugationResultEnum.Incorrect => UiBrushes.TextBoxIncorrect,
            ConjugationResultEnum.Close => UiBrushes.TextBoxClose,
            _ => UiBrushes.TextBoxDefault
        };

        public bool IsCheckEnabled => Result != ConjugationResultEnum.Correct;
        public bool IsHintEnabled => Result != ConjugationResultEnum.Correct;

        public void RefreshFromState()
        {
            UserInput = State.UserInput ?? string.Empty;
            Result = State.Result;
        }
    }
}
