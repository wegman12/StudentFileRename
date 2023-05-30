using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;
using Ghostscript.NET.Rasterizer;
using Ghostscript.NET;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using PdfDocument = UglyToad.PdfPig.PdfDocument;

namespace StudentFileRename;

public partial class MainPage : ContentPage, IDisposable
{
    private CancellationTokenSource InputFolderCancellationTokenSource;
    private CancellationTokenSource OutputFolderCancellationTokenSource;
    private TesseractEngine Engine;
    private GhostscriptRasterizer Rasterizer;

    public MainPage()
    {
        InitializeComponent();
        InputFolderCancellationTokenSource = new CancellationTokenSource();
        OutputFolderCancellationTokenSource = new CancellationTokenSource();
        Engine = new TesseractEngine($"{BinDir}\\tessdata", "eng", EngineMode.Default);
        Rasterizer = new GhostscriptRasterizer();
    }

    private void OnInputFolderClicked(object sender, EventArgs e)
    {
        PickInputFolder();
    }

    private async Task PickInputFolder()
    {
        InputFolderCancellationTokenSource = new CancellationTokenSource();
        var result = await FolderPicker.Default.PickAsync(InputFolderCancellationTokenSource.Token);
        InputFolderLabel.Text = result?.Folder?.Path;
        SetProcessButtonDisabled();
    }

    private void OnOutputFolderClicked(object sender, EventArgs e)
    {
        PickOutputFolder();
    }

    private async Task PickOutputFolder()
    {
        OutputFolderCancellationTokenSource = new CancellationTokenSource();
        var result = await FolderPicker.Default.PickAsync(OutputFolderCancellationTokenSource.Token);
        OutputFolderLabel.Text = result?.Folder?.Path;
        SetProcessButtonDisabled();
    }

    private void SetProcessButtonDisabled()
    {
        GenerateNewFilesButton.IsEnabled =
            !string.IsNullOrWhiteSpace(InputFolderLabel.Text) && !string.IsNullOrWhiteSpace(OutputFolderLabel.Text);
    }

    private void OnGenerateNewFiles(object sender, EventArgs e)
    {
        GenerateNewFiles();
    }

    private async Task GenerateNewFiles()
    {
        var token = new CancellationTokenSource();

        GenerateNewFilesButton.IsVisible = false;
        InputFolderButton.IsEnabled = false;
        OutputFolderButton.IsEnabled = false;
        GenerateNewFilesProgressBar.IsVisible = true;
        FileProgressLabel.IsVisible = true;

        try
        {
            await DoFileProcessing();
        }
        catch (Exception ex)
        {
            GenerateNewFilesProgressBar.Progress = 1;
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            GenerateNewFilesProgressBar.Progress = 0;

            InputFolderLabel.Text = null;
            OutputFolderLabel.Text = null;
            GenerateNewFilesButton.IsVisible = true;
            GenerateNewFilesProgressBar.IsVisible = false;
            FileProgressLabel.IsVisible = false;
            FileProgressLabel.Text = "";
            InputFolderButton.IsEnabled = true;
            OutputFolderButton.IsEnabled = true;
        }
    }

    private async Task DoFileProcessing()
    {
        var files = Directory.EnumerateFiles(InputFolderLabel.Text).Where(f => f.Split(".").Last().ToLower() == "pdf").ToArray();

        if (!files.Any())
        {
            throw new ApplicationException("No pdf files found in input folder - please ensure input and try again!");
        }

        int currentFile = 0;

        var exceptions = new List<Exception>();

        foreach (var file in files)
        {
            try
            {
                FileProgressLabel.Text =
                    $"Processing File {currentFile + 1} of {files.Length}: {Path.GetFileName(file)}";
                await ProcessFile(file);
            }
            catch (Exception ex)
            {
                exceptions.Add(new AggregateException($"Failed to process: {file}", ex));
            }
            finally
            {
                GenerateNewFilesProgressBar.Progress = ((double)currentFile++) / files.Length;
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }

    private Task ProcessFile(string file)
    {
        return Task.Run(() => {
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
            File.Copy(file, Path.Join(OutputFolderLabel.Text, $"{studentId}.pdf"), true);
        });
    }

    private string TryParseImage(string file)
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
    private void PdfToPng(string inputFile, string outputFileName)
    {
            var pageNumber = 1; // the pages in a PDF document

            
            var fName = "gsdll64.dll";
            if (IntPtr.Size == 4)
            {
                fName = "gsdll32.dll";
            }

            Rasterizer.Open(inputFile, new GhostscriptVersionInfo($"{BinDir}\\{fName}"),
                false); //opens the PDF file for rasterizing


            //converts the PDF pages to png's 
            var pdf2PNG = Rasterizer.GetPage(800, 800, pageNumber);

            //save the png's
            pdf2PNG.Save(outputFileName, ImageFormat.Png);
    }

    private string ReadImage(string imageName)
    {
        using var img = Pix.LoadFromFile(imageName);
        using var page = Engine.Process(img);
        return page.GetText();
    }
    private static string BinDir => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    public void Dispose()
    {
        Engine?.Dispose();
        Rasterizer?.Dispose();
    }
}