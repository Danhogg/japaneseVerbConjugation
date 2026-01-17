using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using JapaneseVerbConjugation.AvaloniaUI.Constants;
using Avalonia.Threading;
using JapaneseVerbConjugation.AvaloniaUI.Infrastructure;
using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Constants;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace JapaneseVerbConjugation.AvaloniaUI.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly Window _owner;
        private readonly VerbStudySession _session;
        private readonly List<ConjugationEntryState> _entryStates = [];

        private string _currentVerb = "Import verbs to begin";
        private string _furiganaReading = string.Empty;
        private bool _canPrev;
        private bool _canNext;
        private bool _verbGroupLocked;
        private VerbGroupEnum? _selectedVerbGroup;
        private bool? _verbGroupIsCorrect;

        public MainViewModel(Window owner)
        {
            _owner = owner;
            _session = VerbStudySession.LoadFromStorage();

            Entries = new ObservableCollection<ConjugationEntryViewModel>();

            CheckEntryCommand = new RelayCommand(CheckEntry);
            HintEntryCommand = new RelayCommand(ShowHint);
            NextVerbCommand = new RelayCommand(_ => MoveNext(), _ => CanNext);
            PrevVerbCommand = new RelayCommand(_ => MovePrev(), _ => CanPrev);
            CheckVerbGroupCommand = new RelayCommand(_ => CheckVerbGroup(), _ => IsCheckVerbGroupEnabled);
            SelectVerbGroupCommand = new RelayCommand(SelectVerbGroup);
            OptionsCommand = new RelayCommand(async _ => await ShowOptionsAsync());
            ImportCommand = new RelayCommand(async _ => await ShowImportAsync());
            ClearCommand = new RelayCommand(_ => ShowInfo("Not implemented", "Clear is not implemented yet."), _ => IsClearEnabled);

            WindowTitle = VersionInfo.GetApplicationTitle();

            BuildEntries();

            var initial = _session.GetInitialVerb();
            if (initial != null)
            {
                LoadVerb(initial);
            }
            else
            {
                SetStudyUiEnabled(false);
            }
        }

        public string WindowTitle { get; }

        public ObservableCollection<ConjugationEntryViewModel> Entries { get; }

        public string CurrentVerb
        {
            get => _currentVerb;
            private set => SetProperty(ref _currentVerb, value);
        }

        public string FuriganaReading
        {
            get => _furiganaReading;
            private set => SetProperty(ref _furiganaReading, value);
        }

        public bool ShowFurigana => _session.Options.ShowFurigana;

        public bool CanPrev
        {
            get => _canPrev;
            private set
            {
                if (SetProperty(ref _canPrev, value))
                    RaiseNavigationCanExecute();
            }
        }

        public bool CanNext
        {
            get => _canNext;
            private set
            {
                if (SetProperty(ref _canNext, value))
                    RaiseNavigationCanExecute();
            }
        }

        public bool IsClearEnabled => false;

        public bool IsCheckVerbGroupEnabled => !_verbGroupLocked;
        public bool IsVerbGroupHitTestEnabled => !_verbGroupLocked;

        public bool IsGodanSelected => _selectedVerbGroup == VerbGroupEnum.Godan;
        public bool IsIchidanSelected => _selectedVerbGroup == VerbGroupEnum.Ichidan;
        public bool IsIrregularSelected => _selectedVerbGroup == VerbGroupEnum.Irregular;

        public Avalonia.Media.IBrush GodanForeground => GetVerbGroupForeground(VerbGroupEnum.Godan);
        public Avalonia.Media.IBrush IchidanForeground => GetVerbGroupForeground(VerbGroupEnum.Ichidan);
        public Avalonia.Media.IBrush IrregularForeground => GetVerbGroupForeground(VerbGroupEnum.Irregular);

        public ICommand CheckEntryCommand { get; }
        public ICommand HintEntryCommand { get; }
        public ICommand NextVerbCommand { get; }
        public ICommand PrevVerbCommand { get; }
        public ICommand CheckVerbGroupCommand { get; }
        public ICommand SelectVerbGroupCommand { get; }
        public ICommand OptionsCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ClearCommand { get; }

        private void BuildEntries()
        {
            Entries.Clear();
            _entryStates.Clear();

            foreach (var form in Enum.GetValues<ConjugationFormEnum>())
            {
                if (!_session.Options.EnabledConjugations.Contains(form))
                    continue;

                var state = new ConjugationEntryState { ConjugationForm = form };
                _entryStates.Add(state);
                Entries.Add(new ConjugationEntryViewModel(state));
            }
        }

        private void LoadVerb(Verb verb)
        {
            _session.LoadVerb(verb, _entryStates);
            CurrentVerb = verb.DictionaryForm;
            FuriganaReading = verb.Reading;

            foreach (var entry in Entries)
            {
                entry.RefreshFromState();
            }

            ApplyVerbGroupState(_session.GetVerbGroupStateForRestore());
            UpdateNavigation();

            OnPropertyChanged(nameof(ShowFurigana));
        }

        private void SetStudyUiEnabled(bool enabled)
        {
            if (!enabled)
            {
                CurrentVerb = "Import verbs to begin";
                FuriganaReading = string.Empty;
                CanPrev = false;
                CanNext = false;
            }
        }

        private void UpdateNavigation()
        {
            var nav = _session.GetNavigationState();
            CanPrev = nav.CanGoPrevious;
            CanNext = nav.CanGoNext;
        }

        private void RaiseNavigationCanExecute()
        {
            if (NextVerbCommand is RelayCommand next)
                next.RaiseCanExecuteChanged();
            if (PrevVerbCommand is RelayCommand prev)
                prev.RaiseCanExecuteChanged();
        }

        private void CheckEntry(object? parameter)
        {
            if (parameter is not ConjugationEntryViewModel entry)
                return;

            _session.CheckAnswer(entry.State);
            entry.RefreshFromState();
        }

        private void ShowHint(object? parameter)
        {
            if (parameter is not ConjugationEntryViewModel entry)
                return;

            var hint = _session.GetHint(entry.State.ConjugationForm);
            if (!hint.HasHint)
            {
                ShowInfo("No hint", "No hint available for this form.");
                return;
            }

            var dialog = new HintWindow(hint.Masked, hint.Full)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.ShowDialog(_owner);
        }

        private void MoveNext()
        {
            if (_session.TryMoveToNext(_entryStates, out var next) && next != null)
            {
                LoadVerb(next);
            }
        }

        private void MovePrev()
        {
            if (_session.TryMoveToPrevious(_entryStates, out var prev) && prev != null)
            {
                LoadVerb(prev);
            }
        }

        private void CheckVerbGroup()
        {
            if (_selectedVerbGroup is null)
            {
                ShowInfo("No selection", "Please select a verb group before checking.");
                return;
            }

            var result = _session.CheckVerbGroup(_selectedVerbGroup.Value);
            _verbGroupIsCorrect = result.IsCorrect;
            _verbGroupLocked = result.IsCorrect;
            OnVerbGroupStateChanged();
        }

        private void ApplyVerbGroupState(VerbGroupState state)
        {
            _selectedVerbGroup = state.Selected;
            _verbGroupIsCorrect = state.IsCorrect;
            _verbGroupLocked = state.LockSelection;
            OnVerbGroupStateChanged();
        }

        private void OnVerbGroupStateChanged()
        {
            OnPropertyChanged(nameof(IsGodanSelected));
            OnPropertyChanged(nameof(IsIchidanSelected));
            OnPropertyChanged(nameof(IsIrregularSelected));
            OnPropertyChanged(nameof(GodanForeground));
            OnPropertyChanged(nameof(IchidanForeground));
            OnPropertyChanged(nameof(IrregularForeground));
            OnPropertyChanged(nameof(IsCheckVerbGroupEnabled));
            OnPropertyChanged(nameof(IsVerbGroupHitTestEnabled));

            if (CheckVerbGroupCommand is RelayCommand cmd)
                cmd.RaiseCanExecuteChanged();
        }

        private void SelectVerbGroup(object? parameter)
        {
            if (parameter is not VerbGroupEnum group)
                return;

            if (_verbGroupLocked)
            {
                // Keep the locked selection visible and prevent any changes.
                OnVerbGroupStateChanged();
                return;
            }

            _selectedVerbGroup = group;
            _verbGroupIsCorrect = null;
            OnVerbGroupStateChanged();
        }

        private Avalonia.Media.IBrush GetVerbGroupForeground(VerbGroupEnum group)
        {
            if (_selectedVerbGroup != group || !_verbGroupIsCorrect.HasValue)
                return UiBrushes.LabelDefault;

            return _verbGroupIsCorrect.Value ? UiBrushes.LabelCorrect : UiBrushes.LabelIncorrect;
        }

        private async Task ShowOptionsAsync()
        {
            var window = new OptionsWindow(_session.Options)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var result = await window.ShowDialog<bool?>(_owner);
            if (result == true)
            {
                var newOptions = window.Result;
                if (!AppOptionsStore.AreEqual(_session.Options, newOptions))
                {
                    AppOptionsStore.Save(newOptions);
                    _session.UpdateOptions(newOptions);
                    BuildEntries();
                    if (_session.CurrentVerb != null)
                    {
                        LoadVerb(_session.CurrentVerb);
                    }
                    OnPropertyChanged(nameof(ShowFurigana));
                }
            }
        }

        private async Task ShowImportAsync()
        {
            var window = new ImportWindow(_session)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var result = await window.ShowDialog<bool?>(_owner);
            if (result == true)
            {
                var initial = _session.GetInitialVerb();
                if (initial != null)
                {
                    LoadVerb(initial);
                }
                else
                {
                    SetStudyUiEnabled(false);
                }
            }
        }

        private void ShowInfo(string title, string message)
        {
            var dialog = new MessageWindow(title, message)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.ShowDialog(_owner);
        }
    }
}
