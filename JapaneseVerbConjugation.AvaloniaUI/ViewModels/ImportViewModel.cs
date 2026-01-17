using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using JapaneseVerbConjugation.SharedResources.Constants;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace JapaneseVerbConjugation.AvaloniaUI.ViewModels
{
    public sealed class ImportViewModel : ViewModelBase
    {
        private bool _importN5 = true;
        private bool _importN4;
        private bool _importCustom;
        private bool _isImporting;
        private readonly VerbStudySession _session;

        public ImportViewModel(VerbStudySession session)
        {
            _session = session;
            CustomCsvPath = CustomCsvStore.EnsureExists();
            LogLines = new ObservableCollection<ImportLogLine>();
        }

        public bool ImportN5
        {
            get => _importN5;
            set => SetProperty(ref _importN5, value);
        }

        public bool ImportN4
        {
            get => _importN4;
            set => SetProperty(ref _importN4, value);
        }

        public bool ImportCustom
        {
            get => _importCustom;
            set => SetProperty(ref _importCustom, value);
        }

        public bool IsImporting
        {
            get => _isImporting;
            set
            {
                if (SetProperty(ref _isImporting, value))
                {
                    OnPropertyChanged(nameof(IsImportEnabled));
                }
            }
        }

        public bool IsImportEnabled => !IsImporting;

        public string CustomCsvPath { get; }

        public ObservableCollection<ImportLogLine> LogLines { get; }

        public async Task<bool> RunImportAsync()
        {
            if (IsImporting)
                return false;

            IsImporting = true;
            LogLines.Clear();
            AppendLog("Starting import...", Brushes.Gray);

            var files = new List<string>();
            if (ImportN5)
            {
                var resolved = DataPathProvider.TryResolveDataFile(DataFileConstants.N5CsvFileName);
                if (resolved != null) files.Add(resolved);
                else AppendLog($"Missing file: {DataFileConstants.N5CsvFileName} (Data folder not found)", Brushes.Red);
            }
            if (ImportN4)
            {
                var resolved = DataPathProvider.TryResolveDataFile(DataFileConstants.N4CsvFileName);
                if (resolved != null) files.Add(resolved);
                else AppendLog($"Missing file: {DataFileConstants.N4CsvFileName} (Data folder not found)", Brushes.Red);
            }
            if (ImportCustom) files.Add(CustomCsvPath);

            bool anyImported = false;

            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    if (!File.Exists(file))
                    {
                        AppendLog($"Missing file: {file}", Brushes.Red);
                        continue;
                    }

                    AppendLog($"Importing: {Path.GetFileName(file)}", Brushes.DimGray);

                    VerbImportService.ImportFromDelimitedFile(file, _session.Store, ev =>
                    {
                        AppendLog(ev.Message, ev.Status switch
                        {
                            VerbImportService.RowStatus.Added => Brushes.Green,
                            VerbImportService.RowStatus.Duplicate => Brushes.Orange,
                            VerbImportService.RowStatus.Error => Brushes.Red,
                            _ => Brushes.Gray
                        });
                    });

                    VerbStoreStore.Save(_session.Store);
                    anyImported = true;
                }
            });

            AppendLog("Import complete.", Brushes.Gray);
            IsImporting = false;
            return anyImported;
        }

        public void OpenCustomFile()
        {
            TryOpen(CustomCsvPath);
        }

        public void OpenCustomFolder()
        {
            var dir = Path.GetDirectoryName(CustomCsvPath);
            if (string.IsNullOrWhiteSpace(dir))
                return;

            TryOpen(dir);
        }

        private void TryOpen(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch
            {
                AppendLog($"Could not open: {path}", Brushes.Red);
            }
        }

        // DataPathProvider now owns data root resolution for all UIs.

        private void AppendLog(string message, IBrush colour)
        {
            Dispatcher.UIThread.Post(() =>
                LogLines.Add(new ImportLogLine(message, colour)));
        }
    }
}
