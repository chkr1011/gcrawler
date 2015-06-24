namespace PiVi
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Windows.Input;

    public partial class MainWindow
    {
        private readonly MemoryStream _selectedImageStream = new MemoryStream();
        private readonly MainWindowViewModel _vm = new MainWindowViewModel();
        private Image _imageInstance;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += delegate { this._buttonNextImage.Focus(); };

            this.DataContext = this._vm;
            this._vm.PropertyChanged += this.OnVMPropertyChanged;
        }

        private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName != "SelectedImage")
            {
                return;
            }

            byte[] buffer = File.ReadAllBytes(this._vm.SelectedImage.Filename);
            this._selectedImageStream.SetLength(0);
            this._selectedImageStream.Write(buffer, 0, buffer.Length);
            
            if (this._imageInstance != null)
            {
                this._imageInstance.Dispose();
            }

            this._imageInstance = System.Drawing.Image.FromStream(this._selectedImageStream);
            this._pictureBox.Image = this._imageInstance;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Right)
            {
                this._vm.CommandShowNextImage.Execute(null);
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Left)
            {
                this._vm.CommandShowPreviousImage.Execute(null);
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Delete)
            {
                this._vm.CommandDeleteSelectedImage.Execute(null);
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Escape)
            {
                Environment.Exit(0);
            }
        }

        private void OnChooseFolderMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            using (var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                this._vm.SetImageLocation(folderBrowserDialog.SelectedPath);
            }
        }
    }
}
