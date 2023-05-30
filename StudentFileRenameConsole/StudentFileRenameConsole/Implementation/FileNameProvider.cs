using StudentFileRenameConsole.Interface;
using System.Reflection;
using System.Text.RegularExpressions;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using Tesseract;
using UglyToad.PdfPig;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace StudentFileRenameConsole.Implementation;

public class FileNameProvider: IFileNameProvider
{
    private readonly GhostscriptRasterizer _rasterizer;
    private readonly string _binDir;
    private readonly TesseractEngine _engine;

    public FileNameProvider()
    {
        _rasterizer = new GhostscriptRasterizer();
        _binDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? throw new ApplicationException("Couldn't find this");
        _engine = new TesseractEngine($"{_binDir}\\tessdata", "eng", EngineMode.Default);
    }

    public string GetNameOfFile(string fileName)
    {
        using var pdf = PdfDocument.Open(fileName);
        var text = pdf.GetPage(1).Text;
        var pattern = @"Student Id: (\d+)";
        var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
        if (matches.Count == 0)
        {
            text = TryParseImage(fileName);
            matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
            if (matches.Count == 0)
            {
                throw new ApplicationException($"No student id found for file {fileName}");
            }
        }

        if (matches.Count > 1)
        {
            throw new ApplicationException($"Multiple student id matches found for file {fileName}");
        }

        var studentId = matches[0].Groups[1];
        return $"{studentId}";
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

        _rasterizer.Open(inputFile, new GhostscriptVersionInfo($"{_binDir}\\{fName}"),
            false); //opens the PDF file for rasterizing


        //converts the PDF pages to png's 
        var pdf2PNG = _rasterizer.GetPage(800, 800, 1);

        //save the png's
        pdf2PNG.Save(outputFileName, ImageFormat.Png);
    }

    private string ReadImage(string imageName)
    {
        using var img = Pix.LoadFromFile(imageName);
        using var page = _engine.Process(img);
        return page.GetText();
    }
}