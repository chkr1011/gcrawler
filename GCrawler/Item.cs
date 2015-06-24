using System;

namespace GCrawler
{
    internal sealed class Item
    {
        public Item(Uri source, string tempFilename)
        {
            Source = source;
            TempFilename = tempFilename;
        }

        public Uri Source { get; private set; }

        public string TempFilename { get; private set; }
    }
}
