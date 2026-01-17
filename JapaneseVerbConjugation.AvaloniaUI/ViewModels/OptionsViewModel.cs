using System;
using System.Collections.ObjectModel;
using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.SharedResources.Constants;

namespace JapaneseVerbConjugation.AvaloniaUI.ViewModels
{
    public sealed class OptionsViewModel : ViewModelBase
    {
        private bool _showFurigana;
        private bool _allowHiragana;
        private bool _focusModeOnly;

        public OptionsViewModel(AppOptions current)
        {
            _showFurigana = current.ShowFurigana;
            _allowHiragana = current.AllowHiragana;
            _focusModeOnly = current.FocusModeOnly;

            Conjugations = new ObservableCollection<ConjugationOptionViewModel>();
            foreach (var form in Enum.GetValues<ConjugationFormEnum>())
            {
                if (form.ToString() == ConjugationNameConstants.DictionaryFormConst)
                    continue;

                Conjugations.Add(new ConjugationOptionViewModel(form, current.EnabledConjugations.Contains(form)));
            }
        }

        public ObservableCollection<ConjugationOptionViewModel> Conjugations { get; }

        public bool ShowFurigana
        {
            get => _showFurigana;
            set => SetProperty(ref _showFurigana, value);
        }

        public bool AllowHiragana
        {
            get => _allowHiragana;
            set => SetProperty(ref _allowHiragana, value);
        }

        public bool FocusModeOnly
        {
            get => _focusModeOnly;
            set => SetProperty(ref _focusModeOnly, value);
        }

        public AppOptions BuildResult()
        {
            var options = new AppOptions
            {
                ShowFurigana = ShowFurigana,
                AllowHiragana = AllowHiragana,
                FocusModeOnly = FocusModeOnly
            };

            options.EnabledConjugations.Clear();
            foreach (var conj in Conjugations)
            {
                if (conj.IsEnabled)
                    options.EnabledConjugations.Add(conj.Form);
            }

            return options;
        }
    }
}
