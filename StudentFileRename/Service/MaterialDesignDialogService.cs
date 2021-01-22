using StudentFileRename.Interface;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StudentFileRename.Service
{
    public class MaterialDesignDialogService : IDialogService
    {
        public async Task<object> ShowDialog(object content)
        {
            return await DialogHost.Show(content);
        }
    }
}
