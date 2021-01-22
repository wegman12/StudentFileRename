namespace StudentFileRename.ViewModel
{
    public class InformationalDialogViewModel
    {
        public string Message { get; }
        public string Title { get; }

        public InformationalDialogViewModel(string title, string message)
        {
            Message = message;
            Title = title;
        }
    }
}