using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PiVi
{
    public partial class MainWindow
    {
        private readonly MemoryStream _selectedImageStream = new MemoryStream();
        private readonly MainWindowViewModel _vm = new MainWindowViewModel();
        private Image _imageInstance;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += delegate { this._buttonNextImage.Focus(); };

            DataContext = _vm;
            _vm.PropertyChanged += OnVMPropertyChanged;
        }

        private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName != "SelectedImage")
            {
                return;
            }

            byte[] buffer = File.ReadAllBytes(_vm.SelectedImage.Filename);
            _selectedImageStream.SetLength(0);
            _selectedImageStream.Write(buffer, 0, buffer.Length);
            
            if (_imageInstance != null)
            {
                _imageInstance.Dispose();
            }

            _imageInstance = Image.FromStream(_selectedImageStream);
            _pictureBox.Image = _imageInstance;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Right)
            {
                _vm.CommandShowNextImage.Execute(null);
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Left)
            {
                _vm.CommandShowPreviousImage.Execute(null);
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Delete)
            {
                _vm.CommandDeleteSelectedImage.Execute(null);
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Escape)
            {
                Environment.Exit(0);
            }
        }

        private void OnChooseFolderMouseDown(object sender, MouseButtonEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                _vm.SetImageLocation(folderBrowserDialog.SelectedPath);
            }
        }
    }
}
