namespace GCrawler
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class ItemProcessor
    {
        private readonly Size _minSize = new Size(64, 64);
        private readonly int _minFramesCount = 5;

        private readonly List<Uri> _sources = new List<Uri>();
        private readonly List<string> _blockedSources = new List<string>();
        private readonly List<Task> _workerTasks = new List<Task>();

        public ItemProcessor()
        {
            for (int taskCount = 0; taskCount < 3; taskCount++)
            {
                var task = new Task(this.ProcessSources, TaskCreationOptions.LongRunning);
                task.Start();

                this._workerTasks.Add(task);
            }
        }

        public void QueueItems(IEnumerable<Uri> sources)
        {
            lock (this._sources)
            {
                foreach (Uri source in sources)
                {
                    string sourceText = source.OriginalString.ToLower();

                    if (this._blockedSources.Contains(sourceText))
                    {
                        Tracer.WriteVerbose("Skipped item '{0}' because it has already been processed.", source);
                        continue;
                    }

                    this._sources.Add(source);
                    this._blockedSources.Add(sourceText);

                    Tracer.WriteVerbose("Added item ({0}) to the item processing queue.", source);
                }
            }
        }

        private void ProcessSources()
        {
            while (true)
            {
                var itemsToLoad = new List<Uri>();
                lock (this._sources)
                {
                    itemsToLoad.AddRange(this._sources);
                    this._sources.Clear();
                }

                if (itemsToLoad.Count == 0)
                {
                    Thread.Sleep(1000);
                }

                foreach (Uri itemSource in itemsToLoad)
                {
                    try
                    {
                        Tracer.WriteVerbose("Initiated download of item from ({0}).", itemSource);
                        Item item = ContentDownloader.DownloadItem(itemSource);
                        this.OnItemDownloaded(item);
                    }
                    catch (Exception exception)
                    {
                        Tracer.WriteHint("Could not process item ({0}). {1}", itemSource, exception.Message);
                    }
                }
            }
        }

        private bool ValidateItem(Item item)
        {
            try
            {
                using (Image image = Image.FromFile(item.TempFilename))
                {
                    if (!image.RawFormat.Guid.Equals(ImageFormat.Gif.Guid))
                    {
                        Tracer.WriteVerbose("Item from '{0}' is an image but no GIF.", item.Source);
                        return false;
                    }

                    if (image.Size.Width < this._minSize.Width || image.Size.Height < this._minSize.Height)
                    {
                        Tracer.WriteVerbose("Item from '{0}' is too small.", item.Source);
                        return false;
                    }

                    if (image.FrameDimensionsList.Length == 0)
                    {
                        Tracer.WriteVerbose("Item from '{0}' has no frame dimension list.", item.Source);
                        return false;
                    }

                    if (image.GetFrameCount(new FrameDimension(image.FrameDimensionsList[0])) < this._minFramesCount)
                    {
                        Tracer.WriteVerbose("Item from '{0}' is not animated.", item.Source);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Tracer.WriteWarning("Item from '{0}' is no image. {1}", item.Source, exception.Message);
                return false;
            }
        }

        private void OnItemDownloaded(Item item)
        {
            if (this.ValidateItem(item))
            {
                ContentManager.SaveContent(item, item.Source);
            }

            File.Delete(item.TempFilename);
            Tracer.WriteVerbose("Completed processing of item '{0}'.", item.Source);
        }
    }
}
