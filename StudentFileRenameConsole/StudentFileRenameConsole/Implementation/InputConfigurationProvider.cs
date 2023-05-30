using StudentFileRenameConsole.DataModel;
using StudentFileRenameConsole.Interface;
using System;
using Newtonsoft.Json;

namespace StudentFileRenameConsole.Implementation;

public class InputConfigurationProvider: IInputConfigurationProvider
{
    public InputConfiguration GetInputConfigurationFromArguments(string[] arguments)
    {

        if (arguments.Length != 1)
        {
            throw new ApplicationException("Application expects single input argument to input configuration file");
        }

        if (!File.Exists(arguments[0]))
        {
            throw new ApplicationException($"Could not find input file structure at: {arguments[0]}");
        }

        var text = File.ReadAllText(arguments[0]);
        var converted = JsonConvert.DeserializeObject<InputConfiguration>(text);
        if (converted == null)
        {
            throw new ApplicationException($"File is in wrong format: {arguments[0]}");
        }
        return converted;
    }
}