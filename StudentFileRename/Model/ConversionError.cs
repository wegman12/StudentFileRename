using System;

namespace StudentFileRename.Model
{
    public class ConversionError
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public Exception Error { get; set; }
    }
}