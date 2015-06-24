namespace GCrawler
{
    using System;

    internal sealed class Page
    {
        private readonly Uri _source;
        private readonly string _content;

        public Page(Uri source, string content)
        {
            this._source = source;
            this._content = content;
        }

        public Uri Source
        {
            get
            {
                return this._source;
            }
        }

        public string Conent
        {
            get
            {
                return this._content;
            }
        }
    }
}
