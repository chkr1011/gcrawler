namespace GCrawler
{
    using System;
    using System.Threading;

    public static class Program
    {
        private static readonly PageProcessor _pageProcessor = new PageProcessor();
        private static readonly ItemProcessor _itemProcessor = new ItemProcessor();

        private static void Main()
        {
            Tracer.WriteInformation("Application started.");

            Program._pageProcessor.NewItemSourcesAvailable += sources => Program._itemProcessor.QueueItems(sources);

            Program._pageProcessor.QueueSource(new Uri("http://www.gifbin.com/"));
            
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
