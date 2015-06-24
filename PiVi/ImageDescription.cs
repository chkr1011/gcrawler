namespace PiVi
{
    using System.IO;

    internal class ImageDescription
    {
        private readonly string _filename;
        private readonly FileInfo _fileInfo;

        public ImageDescription(string filename)
        {
            this._filename = filename;
            this._fileInfo = new FileInfo(filename);
        }

        public string Filename
        {
            get
            {
                return this._filename;
            }
        }

        public FileInfo FileInfo
        {
            get
            {
                return this._fileInfo;
            }
        }
    }
}
