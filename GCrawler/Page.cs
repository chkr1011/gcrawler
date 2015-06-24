using System;

namespace GCrawler
{
    internal sealed class Page
    {
        public Page(Uri source, string content)
        {
            Source = source;
            Content = content;
        }

        public Uri Source { get; private set; }

        public string Content { get; private set; }
    }
}
