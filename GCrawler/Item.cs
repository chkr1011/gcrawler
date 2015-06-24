namespace GCrawler
{
    using System;

    internal sealed class Item
    {
        private readonly Uri _source;
        private readonly string _tempFilename;

        public Item(Uri source, string tempFilename)
        {
            this._source = source;
            this._tempFilename = tempFilename;
        }

        public Uri Source
        {
            get
            {
                return this._source;
            }
        }

        public string TempFilename
        {
            get
            {
                return this._tempFilename;
            }
        }
    }
}
