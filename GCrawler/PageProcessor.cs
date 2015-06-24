namespace GCrawler
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class PageProcessor
    {
        private readonly Regex _imgTagRegex;
        private readonly Regex _srcAttributeRegex;
        private readonly Regex _aTagRegex;
        private readonly Regex _hrefAttributeRegex;

        private readonly List<PageRequest> _requests = new List<PageRequest>();
        private readonly List<string> _blockedSources = new List<string>();
        private readonly List<Task> _workerTasks = new List<Task>();

        private readonly List<string> _excludingUriPaths = new List<string>(); 

        public PageProcessor()
        {
            this._imgTagRegex = this.GenerateRegExForTag("img");
            this._srcAttributeRegex = this.GenerateRegExForAttribute("src");
            this._aTagRegex = this.GenerateRegExForTag("a");
            this._hrefAttributeRegex = this.GenerateRegExForAttribute("href");
            
            for (int taskCount = 0; taskCount < 10; taskCount++)
            {
                var task = new Task(this.ProcessSources, TaskCreationOptions.LongRunning);
                task.Start();

                this._workerTasks.Add(task);
            }
        }

        public event Action<List<Uri>> NewItemSourcesAvailable;

        private List<Uri> ExtractItemSources(Page page)
        {
            List<Uri> sources = this.ExtractSources(
                page,
                this._imgTagRegex,
                this._srcAttributeRegex,
                source => source.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase));

            sources.AddRange(this.ExtractSources(
                page,
                this._aTagRegex,
                this._hrefAttributeRegex,
                source => source.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase)));
    
            return sources;
        }

        private List<Uri> ExtractPageSources(Page page)
        {
            return this.ExtractSources(
                page,
                this._aTagRegex,
                this._hrefAttributeRegex,
                delegate(string source)
                    {
                        foreach (string excludingUriPath in this._excludingUriPaths)
                        {
                            if (source.StartsWith(excludingUriPath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return false;
                            }
                        }

                        return true;
                    });
        }

        public void QueueSource(Uri source)
        {
            lock (this._requests)
            {
                if (this._blockedSources.Contains(source.OriginalString.ToLower()))
                {
                    Tracer.WriteHint("Skipping add of source '{0}' because it is already scanned.", source);
                    return;
                }

                this._requests.Add(new PageRequest(source));
                this._blockedSources.Add(source.OriginalString.ToLower());
                Tracer.WriteHint("Added manual source '{0}'.", source);
            }
        }

        private void ProcessSources()
        {
            while (true)
            {
                var requests = new List<PageRequest>();
                lock (this._requests)
                {
                    int range = 25;
                    if (range > this._requests.Count)
                    {
                        range = this._requests.Count;
                    }

                    requests.AddRange(this._requests.GetRange(0, range));
                    this._requests.RemoveRange(0, range);
                }

                if (requests.Count == 0)
                {
                    Thread.Sleep(1000);
                }

                foreach (PageRequest request in requests)
                {
                    if (request.Hops.Count > 5)
                    {
                        Tracer.WriteHint("Excluded page ({0}) because it reaches the max hops count.", request.Source);
                        continue;
                    }

                    try
                    {
                        Tracer.WriteVerbose("Initiated download of page ({0}).", request.Source);
                        Page page = ContentDownloader.DownloadPage(request.Source);

                        List<Uri> itemSources = this.ExtractItemSources(page);
                        Tracer.WriteVerbose("Extraced {0} items sources from page '{1}'.", itemSources.Count, page.Source);

                        List<Uri> subPageSources = this.ExtractPageSources(page);
                        
                        List<PageRequest> subPageRequests =
                            subPageSources.ConvertAll(
                                delegate(Uri source)
                                    {
                                        var subPageRequest = new PageRequest(source);
                                        subPageRequest.Hops.AddRange(request.Hops);
                                        subPageRequest.Hops.Add(request.Source);
                                        
                                        return subPageRequest;
                                    });

                        if (this.NewItemSourcesAvailable != null)
                        {
                            this.NewItemSourcesAvailable(itemSources);
                        }

                        lock (this._requests)
                        {
                            foreach (PageRequest subPageRequest in subPageRequests)
                            {
                            	string sourceText = subPageRequest.Source.OriginalString.ToLower();
                                if (this._blockedSources.Contains(sourceText))
                                {
                                    continue;
                                }

                                this._requests.Add(subPageRequest);
                                this._blockedSources.Add(sourceText);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Tracer.WriteHint("Could not download page from '{0}'. {1}", request, exception.Message);
                    }
                }
            }
        }

        private List<Uri> ExtractSources(Page page, Regex tagRegex, Regex attributeRegex, Func<string, bool> onCheckSource = null)
        {
            var sources = new List<Uri>();
            foreach (Match tagMatch in tagRegex.Matches(page.Conent))
            {
                Match attributeMatch = attributeRegex.Match(tagMatch.Value);
                if (attributeMatch.Groups.Count != 2)
                {
                    continue;
                }

                string attributeValue = attributeMatch.Groups[1].Value;
                if (!Uri.IsWellFormedUriString(attributeValue, UriKind.Absolute))
                {
                    string leftPart = page.Source.GetLeftPart(UriPartial.Authority);
                    if (!leftPart.EndsWith("/") && !attributeValue.StartsWith("/"))
                    {
                        leftPart += "/";
                    }

                    attributeValue = leftPart + attributeValue;
                }

                if (onCheckSource != null)
                {
                    if (!onCheckSource(attributeValue))
                    {
                        continue;
                    }
                }

                sources.Add(new Uri(attributeValue));
            }

            return sources;
        }

        private Regex GenerateRegExForTag(string tag)
        {
            string pattern = string.Format(@"<\s*{0}\s*[\w|\s|""|\'|/|=|\.|:]*>", tag);
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private Regex GenerateRegExForAttribute(string attribute)
        {
            string pattern = string.Format(@"{0}=[""|\']?([\w|%|/|\.|0-6|:]*)[""|\']?", attribute);
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    }
}
