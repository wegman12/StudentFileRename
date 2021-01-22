using System.Windows.Input;
using StudentFileRename.Interface;
using StudentFileRename.Utility;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace StudentFileRename.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public const string StartupPageKey = "STARTUP";
        private BaseViewModel _currentPageViewModel;
        private Dictionary<string, BaseViewModel> _pageViewModels;

        public BaseViewModel CurrentPageViewModel { 
            get => _currentPageViewModel; 
            set
            {
                _currentPageViewModel = value;
                OnPropertyChanged(nameof(CurrentPageViewModel));
            }
        }

        public Dictionary<string, BaseViewModel> PageViewModels { 
            get
            {
                return _pageViewModels ??= new Dictionary<string, BaseViewModel>();
            }
        }


        public MainWindowViewModel(StartupPageViewModel startupPage)
        {
            PageViewModels.Add(StartupPageKey, startupPage);

            CurrentPageViewModel = startupPage;
        }

        private void OpenPageByName(string key)
        {
            CurrentPageViewModel = PageViewModels[key];
        }
    }
}
