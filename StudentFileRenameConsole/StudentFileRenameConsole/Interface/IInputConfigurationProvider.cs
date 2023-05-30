using StudentFileRenameConsole.DataModel;

namespace StudentFileRenameConsole.Interface;

public interface IInputConfigurationProvider
{
    public InputConfiguration GetInputConfigurationFromArguments(string[] arguments);
}