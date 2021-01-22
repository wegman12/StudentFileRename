using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StudentFileRename.Interface
{
    public interface IDialogService
    {
        public Task<object> ShowDialog(object content);
    }
}
