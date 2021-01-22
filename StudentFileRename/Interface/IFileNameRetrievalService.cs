using System;
using System.Collections.Generic;
using System.Text;

namespace StudentFileRename.Interface
{
    public interface IFileNameRetrievalService
    {
        string FindFilePathName(string filter, bool isFolder);
    }
}
