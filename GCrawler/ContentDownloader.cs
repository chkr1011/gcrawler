namespace GCrawler
{
    using System;
    using System.IO;
    using System.Net;

    internal static class ContentDownloader
    {
        private static readonly WebClient _webClient = new WebClient();

        public static Page DownloadPage(Uri source)
        {
            using (var webClient = new ExtendedWebClient())
            {
                Tracer.WriteHint("Start download of page from '{0}'.", source);
                string content = webClient.DownloadString(source);
                Tracer.WriteHint("Completed download of page from '{0}'.", source);

                return new Page(source, content);
            }

            ////lock (ContentDownloader._webClient)
            ////{
            ////    Tracer.WriteHint("Start download of page from '{0}'.", source);
            ////    string content = ContentDownloader._webClient.DownloadString(source);
            ////    Tracer.WriteHint("Completed download of page from '{0}'.", source);

            ////    return new Page(source, content);
            ////}
        }

        public static Item DownloadItem(Uri source)
        {
            using (var webClient = new WebClient())
            {
                string tempFilename = Path.GetTempFileName();

                Tracer.WriteHint("Start download of item from '{0}'.", source);
                webClient.DownloadFile(source, tempFilename);
                Tracer.WriteHint("Completed download of item from '{0}'.", source);

                return new Item(source, tempFilename);
            }

            ////lock (ContentDownloader._webClient)
            ////{
            ////    Tracer.WriteHint("Start download of item from '{0}'.", source);
            ////    byte[] content = ContentDownloader._webClient.DownloadData(source);
            ////    Tracer.WriteHint("Completed download of item from '{0}'.", source);

            ////    return new Item(source, content);
            ////}
        }
    }
}
