using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Ghostscript.NET;
using StudentFileRename.Interface;
using StudentFileRename.Model;
using StudentFileRename.Utility;
using Ghostscript.NET.Rasterizer;
using Ghostscript.NET.Viewer;
using iText.Forms.Xfdf;
using Tesseract;
using UglyToad.PdfPig;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

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
                var errors = new List<ConversionError>();
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
                        try
                        {
                            using var pdf = PdfDocument.Open(file);
                            var text = pdf.GetPage(1).Text;
                            var pattern = @"Student Id: (\d+)";
                            var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                            if (matches.Count == 0)
                            {
                                text = TryParseImage(file);
                                matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                                if (matches.Count == 0)
                                {
                                    throw new ApplicationException($"No student id found for file {file}");
                                }
                            }

                            if (matches.Count > 1)
                            {
                                throw new ApplicationException($"Multiple student id matches found for file {file}");
                            }

                            var studentId = matches[0].Groups[1];
                            File.Copy(file, Path.Join(newDirectory, $"{studentId}.pdf"), true);
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new ConversionError()
                            {
                                Error = ex,
                                FileName = new FileInfo(file).Name,
                                FilePath = file
                            });
                        }
                    }
                });
                if (errors.Any())
                {
                    var fileNames = string.Join(", ", errors.Select(e => e.FileName));
                    var messages = string.Join("\n", errors.Select(e => $"{e.FilePath} - {e.Error.Message}"));
                    dialogModel =
                        new InformationalDialogViewModel("Error",
                            $"Failed to process {errors.Count} file(s): {fileNames}\n\n{messages}");
                }
                else
                {
                    dialogModel = new InformationalDialogViewModel("Success",
                        $"Files were exported to {newDirectory}");
                }
            }
            catch (Exception e)
            {
                dialogModel = new InformationalDialogViewModel("Error",
                    $"The following uncaught exception was encountered: {e}.");
            }
            finally
            {
                IsProcessing = false;
            }

            await _dialogService.ShowDialog(dialogModel);
        }

        private static string TryParseImage(string file)
        {
            var tempImage =
                Environment.ExpandEnvironmentVariables($"%TEMP%\\{Path.GetFileNameWithoutExtension(file)}.png");
            PdfToPng(
                file,
                tempImage
            );

            var text = ReadImage(tempImage);

            File.Delete(tempImage);

            return text;
        }
        private static void PdfToPng(string inputFile, string outputFileName)
        {
            var pageNumber = 1; // the pages in a PDF document
            
                        

            using (var rasterizer = new GhostscriptRasterizer()) //create an instance for GhostscriptRasterizer
            {
                var fName = "gsdll64.dll";
                if (IntPtr.Size == 4)
                {
                    fName = "gsdll32.dll";
                }
                rasterizer.Open(inputFile, new GhostscriptVersionInfo($"{BinDir}\\{fName}"), false); //opens the PDF file for rasterizing
                

                //converts the PDF pages to png's 
                var pdf2PNG = rasterizer.GetPage(200, pageNumber);

                //save the png's
                pdf2PNG.Save(outputFileName, ImageFormat.Png);

            }
        }

        private static string ReadImage(string imageName)
        {
            using var engine = new TesseractEngine($"{BinDir}\\tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imageName);
            using var page = engine.Process(img);
            return page.GetText();
        }

        private static string BinDir => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }
}