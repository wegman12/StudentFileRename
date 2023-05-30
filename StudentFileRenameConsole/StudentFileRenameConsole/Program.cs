// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using Ghostscript.NET.Rasterizer;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentFileRenameConsole.Implementation;
using StudentFileRenameConsole.Interface;
using Tesseract;


var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IInputConfigurationProvider, InputConfigurationProvider>()
            .AddSingleton<IFileNameProvider, FileNameProvider>();
    })
    .Build();


var provider = host.Services.GetRequiredService<IInputConfigurationProvider>();
var converter = host.Services.GetRequiredService<IFileNameProvider>();

var inputConfiguration = provider.GetInputConfigurationFromArguments(args);


if (!Directory.Exists(inputConfiguration.InputFolderStructure))
{
    Console.Error.WriteLine($"Could not find input files at {inputConfiguration.InputFolderStructure}");
}

if (!Directory.Exists(inputConfiguration.OutputFolderStructure))
{
    Directory.CreateDirectory(inputConfiguration.OutputFolderStructure);
}

var filesToConvert = Directory.GetFiles(inputConfiguration.InputFolderStructure, "*.pdf", SearchOption.AllDirectories);

Console.WriteLine($"Found {filesToConvert.Length} files to convert...");

var currentFile = 0;
Console.WriteLine($"0 / {filesToConvert.Length}");

//filesToConvert.AsParallel().ForAll(inputFile =>
foreach (var inputFile in filesToConvert)
{
    try
    {
        var studentId = converter.GetNameOfFile(inputFile);

        var outputFile =
            Path.Join(
                Path.GetDirectoryName(
                    inputFile.Replace(inputConfiguration.InputFolderStructure,
                        inputConfiguration.OutputFolderStructure)),
                $"{studentId}.pdf"
            );

        var outputDirectory = Path.GetDirectoryName(outputFile);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.Copy(inputFile, outputFile, true);
        Console.WriteLine($"{++currentFile} / {filesToConvert.Length}");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine($"Failed to process file {inputFile}: ${e}");
    }
}
