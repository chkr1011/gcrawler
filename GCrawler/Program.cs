using System;
using System.Threading;

namespace GCrawler
{
    public static class Program
    {
        private static readonly PageProcessor PageProcessor = new PageProcessor();
        private static readonly ItemProcessor ItemProcessor = new ItemProcessor();

        private static void Main()
        {
            Tracer.WriteInformation("Application started.");

            PageProcessor.NewItemSourcesAvailable += sources => ItemProcessor.QueueItems(sources);

            foreach (var page in Properties.Settings.Default.Pages)
            {
                PageProcessor.QueueSource(new Uri(page));
            }
            
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
