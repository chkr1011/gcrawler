namespace GCrawler
{
    using System;
    using System.Collections.Generic;

    internal sealed class PageRequest
    {
        private readonly List<Uri> _hops = new List<Uri>();
        private readonly Uri _source;

        public PageRequest(Uri source)
        {
            this._source = source;
        }

        public Uri Source
        {
            get
            {
                return this._source;
            }
        }
        
        public List<Uri> Hops
        {
            get
            {
                return this._hops;
            }
        }
    }
}
