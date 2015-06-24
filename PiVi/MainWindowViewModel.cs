using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace PiVi
{
    internal sealed class MainWindowViewModel : BaseViewModel
    {
        private readonly RelayCommand _commandShowNextImage;
        private readonly RelayCommand _commandShowPreviousImage;
        private readonly RelayCommand _commandDeleteSelectedImage;
        private readonly ObservableCollection<string> _imageFiles = new ObservableCollection<string>();

        private int _imageIndex;
        private ImageDescription _selectedImage;
        
        public MainWindowViewModel()
        {
            _commandShowNextImage = new RelayCommand(ShowNextImgage);
            _commandShowPreviousImage = new RelayCommand(ShowPreviousImage);
            _commandDeleteSelectedImage = new RelayCommand(DeleteSelectedImage);
        }

        public ICommand CommandShowNextImage
        {
            get
            {
                return _commandShowNextImage;
            }
        }

        public ICommand CommandShowPreviousImage
        {
            get
            {
                return _commandShowPreviousImage;
            }
        }

        public ICommand CommandDeleteSelectedImage
        {
            get
            {
                return _commandDeleteSelectedImage;
            }
        }

        public ImageDescription SelectedImage
        {
            get
            {
                return _selectedImage;
            }

            set
            {
                _selectedImage = value;
                OnPropertyChanged(() => SelectedImage);
            }
        }

        public int SelectedImageIndex
        {
            get
            {
                return _imageIndex;
            }

            set
            {
                _imageIndex = value;
                OnPropertyChanged(() => SelectedImageIndex);
            }
        }

        public ObservableCollection<string> ImageFiles
        {
            get
            {
                return _imageFiles;
            }
        }

        private void ShowNextImgage()
        {
            SelectedImageIndex++;
            if (SelectedImageIndex > _imageFiles.Count - 1)
            {
                SelectedImageIndex = 0;
            }

            UpdateSelectedImage();
        }

        private void DeleteSelectedImage()
        {
            if (_selectedImage == null)
            {
                return;
            }

            File.Delete(SelectedImage.Filename);
            _imageFiles.RemoveAt(_imageIndex);

            UpdateSelectedImage();
        }

        private void ShowPreviousImage()
        {
            SelectedImageIndex--;
            if (SelectedImageIndex < 0)
            {
                SelectedImageIndex = _imageFiles.Count - 1;
            }

            UpdateSelectedImage();   
        }

        private void UpdateSelectedImage()
        {
            if (_imageFiles.Count == 0)
            {
                _imageIndex = 0;
                SelectedImage = null;
                return;
            }

            var image = new ImageDescription(_imageFiles[_imageIndex]);
            SelectedImage = image;
        }

        public void SetImageLocation(string selectedPath)
        {
            _imageFiles.Clear();

            foreach (string file in Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories))
            {
                if (!file.ToLowerInvariant().EndsWith(".gif"))
                {
                    continue;
                }

                _imageFiles.Add(file);
            }

            UpdateSelectedImage();
        }
    }
}
