using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using StudentFileRename.Interface;
using StudentFileRename.Model;
using StudentFileRename.Utility;
using UglyToad.PdfPig;

namespace StudentFileRename.ViewModel
{
    public class StartupPageViewModel : BaseViewModel
    {
        private readonly IFileDialogService _fileDialogService;
        private readonly IDialogService _dialogService;

        public StartupPageViewModel(IFileDialogService fileDialogService, IDialogService dialogService)
        {
            _fileDialogService = fileDialogService;
            _dialogService = dialogService;
        }

        public ConversionRequest ConversionRequest { get; set; } = new ConversionRequest();

        public ICommand OnSelectNewFileLocation
        {
            get { return new RelayCommand(param => this.SelectNewFileLocation(), param => true); }
        }

        public ICommand OnSelectOriginalFileLocation
        {
            get { return new RelayCommand(param => this.SelectOriginalFileLocation(), param => true); }
        }

        public ICommand OnCreateNewFiles
        {
            get { return new RelayCommand(param => this.CreateNewFiles(), param => true); }
        }

        private void SelectOriginalFileLocation()
        {
            ConversionRequest.OriginalDirectoryLocation = _fileDialogService.GetFilePathFromExplorer(true);
        }

        private void SelectNewFileLocation()
        {
            ConversionRequest.OutputDirectoryLocation = _fileDialogService.GetFilePathFromExplorer(true);
        }
        private bool _isProcessing;

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        private async void CreateNewFiles()
        {
            IsProcessing = true;
            InformationalDialogViewModel dialogModel;
            try
            {
                var origDirectory = ConversionRequest.OriginalDirectoryLocation;
                var newDirectory = ConversionRequest.OutputDirectoryLocation;
                ConversionRequest.OriginalDirectoryLocation = null;
                ConversionRequest.OutputDirectoryLocation = null;
                await Task.Run(() =>
                {
                    var files = Directory.EnumerateFiles(origDirectory).ToArray();
                    if (!files.Any())
                    {
                        throw new ApplicationException(
                            $"No files were found at {origDirectory}");
                    }

                    foreach (var file in files)
                    {
                        using var pdf = PdfDocument.Open(file);
                        var text = pdf.GetPage(1).Text;
                        var pattern = @"Student Id: (\d+)";
                        var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                        if (matches.Count == 0)
                        {
                            throw new ApplicationException($"No student id found for file {file}");
                        }

                        if (matches.Count > 1)
                        {
                            throw new ApplicationException($"Multiple student id matches found for file {file}");
                        }

                        var studentId = matches[0].Groups[1];
                        File.Copy(file, Path.Join(newDirectory, $"{studentId}.pdf"), true);
                    }
                });
                dialogModel = new InformationalDialogViewModel("Success",
                    $"Files were exported to {newDirectory}");
            }
            catch (Exception e)
            {
                dialogModel = new InformationalDialogViewModel("Error", $"The following uncaught exception was encountered: {e}.");
            }
            finally
            {
                IsProcessing = false;
            }
            await _dialogService.ShowDialog(dialogModel);
            
        }
    }
}