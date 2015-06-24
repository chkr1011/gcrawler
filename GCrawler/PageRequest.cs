using System;
using System.Collections.Generic;

namespace GCrawler
{
    internal sealed class PageRequest
    {
        public PageRequest(Uri source)
        {
            Source = source;
            Hops = new List<Uri>();
        }

        public Uri Source { get; private set; }
        
        public List<Uri> Hops { get; private set; }
    }
}
