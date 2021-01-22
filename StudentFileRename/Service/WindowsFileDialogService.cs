using StudentFileRename.Interface;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentFileRename.Service
{
    public class WindowsFileDialogService : IFileDialogService
    {
        private OpenFileDialog _fileDialog;
        private VistaFolderBrowserDialog _folderDialog;

        public WindowsFileDialogService(OpenFileDialog fileDialog, VistaFolderBrowserDialog folderDialog)
        {
            this._fileDialog = fileDialog;
            this._folderDialog = folderDialog;
        }

        public string GetFilePathFromExplorer(bool isFolder, string filter = null)
        {
            if (isFolder)
            {
                if (_folderDialog.ShowDialog() == true)
                {
                    return _folderDialog.SelectedPath;
                }
                return null;
            }
            else
            {
                _fileDialog.Filter = filter;
                if (_fileDialog.ShowDialog() == true)
                {
                    return _fileDialog.FileName;
                }
                return null;
            }
        }
    }
}
