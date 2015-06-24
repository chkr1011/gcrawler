using System.IO;

namespace PiVi
{
    internal sealed class ImageDescription
    {
        public ImageDescription(string filename)
        {
            Filename = filename;
            FileInfo = new FileInfo(filename);
        }

        public string Filename { get; private set; }

        public FileInfo FileInfo { get; private set; }
    }
}
