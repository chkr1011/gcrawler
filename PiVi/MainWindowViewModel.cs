namespace PiVi
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Input;

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
            this._commandShowNextImage = new RelayCommand(this.ShowNextImgage);
            this._commandShowPreviousImage = new RelayCommand(this.ShowPreviousImage);
            this._commandDeleteSelectedImage = new RelayCommand(this.DeleteSelectedImage);
        }

        public ICommand CommandShowNextImage
        {
            get
            {
                return this._commandShowNextImage;
            }
        }

        public ICommand CommandShowPreviousImage
        {
            get
            {
                return this._commandShowPreviousImage;
            }
        }

        public ICommand CommandDeleteSelectedImage
        {
            get
            {
                return this._commandDeleteSelectedImage;
            }
        }

        public ImageDescription SelectedImage
        {
            get
            {
                return this._selectedImage;
            }

            set
            {
                this._selectedImage = value;
                this.OnPropertyChanged(() => this.SelectedImage);
            }
        }

        public int SelectedImageIndex
        {
            get
            {
                return this._imageIndex;
            }

            set
            {
                this._imageIndex = value;
                this.OnPropertyChanged(() => this.SelectedImageIndex);
            }
        }

        public ObservableCollection<string> ImageFiles
        {
            get
            {
                return this._imageFiles;
            }
        }

        private void ShowNextImgage()
        {
            this.SelectedImageIndex++;
            if (this.SelectedImageIndex > this._imageFiles.Count - 1)
            {
                this.SelectedImageIndex = 0;
            }

            this.UpdateSelectedImage();
        }

        private void DeleteSelectedImage()
        {
            if (this._selectedImage == null)
            {
                return;
            }

            File.Delete(this.SelectedImage.Filename);
            this._imageFiles.RemoveAt(this._imageIndex);

            this.UpdateSelectedImage();
        }

        private void ShowPreviousImage()
        {
            this.SelectedImageIndex--;
            if (this.SelectedImageIndex < 0)
            {
                this.SelectedImageIndex = this._imageFiles.Count - 1;
            }

            this.UpdateSelectedImage();   
        }

        private void UpdateSelectedImage()
        {
            if (this._imageFiles.Count == 0)
            {
                this._imageIndex = 0;
                this.SelectedImage = null;
                return;
            }

            var image = new ImageDescription(this._imageFiles[this._imageIndex]);
            this.SelectedImage = image;
        }

        public void SetImageLocation(string selectedPath)
        {
            this._imageFiles.Clear();

            foreach (string file in Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories))
            {
                if (!file.ToLowerInvariant().EndsWith(".gif"))
                {
                    continue;
                }

                this._imageFiles.Add(file);
            }

            this.UpdateSelectedImage();
        }
    }
}
