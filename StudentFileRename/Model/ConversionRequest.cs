using System.ComponentModel;
using System.IO;

namespace StudentFileRename.Model
{
    public class ConversionRequest : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _originalDirectoryLocation;
        private string _outputDirectoryLocation;
        public string OriginalDirectoryLocation { get => _originalDirectoryLocation;
            set
            {
                _originalDirectoryLocation = value;
                OnPropertyChanged(nameof(OriginalDirectoryLocation));
                OnPropertyChanged(nameof(CanConvert));
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public string OutputDirectoryLocation { get => _outputDirectoryLocation;
            set
            {
                _outputDirectoryLocation = value;
                OnPropertyChanged(nameof(OutputDirectoryLocation));
                OnPropertyChanged(nameof(CanConvert));
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public bool CanConvert => _canConvert();

        public string ErrorMessage => _errorMessage();

        private bool _canConvert()
        {
            if (OriginalDirectoryLocation == null || OutputDirectoryLocation == null)
            {
                return false;
            }

            return Directory.Exists(OriginalDirectoryLocation) && Directory.Exists(OutputDirectoryLocation);
        }

        private string _errorMessage()
        {
            if (OriginalDirectoryLocation == null)
            {
                return "Please set the location of the original files above!";
            }

            if (OutputDirectoryLocation == null)
            {
                return "Please set the location of the output directory above!";
            }

            if (!Directory.Exists((OriginalDirectoryLocation)))
            {
                return "Could not find the location of the original files - please try again!";
            }

            return !Directory.Exists(OutputDirectoryLocation) ? "Could not find the location of the output directory - please try again!" : null;
        }
    }
}