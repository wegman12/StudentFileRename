using System;
using System.Collections.Generic;
using System.Text;

namespace StudentFileRename.Interface
{
    public interface IFileDialogService
    {
        public string GetFilePathFromExplorer(bool isFolder, string filter = null);
    }
}
