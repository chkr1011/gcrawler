using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GCrawler
{
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
            _imgTagRegex = GenerateRegExForTag("img");
            _srcAttributeRegex = GenerateRegExForAttribute("src");
            _aTagRegex = GenerateRegExForTag("a");
            _hrefAttributeRegex = GenerateRegExForAttribute("href");
            
            for (int taskCount = 0; taskCount < 10; taskCount++)
            {
                var task = new Task(ProcessSources, TaskCreationOptions.LongRunning);
                task.Start();

                _workerTasks.Add(task);
            }
        }

        public event Action<List<Uri>> NewItemSourcesAvailable;

        private List<Uri> ExtractItemSources(Page page)
        {
            List<Uri> sources = ExtractSources(
                page,
                _imgTagRegex,
                _srcAttributeRegex,
                source => source.EndsWith(".gif", StringComparison.OrdinalIgnoreCase));

            sources.AddRange(ExtractSources(
                page,
                _aTagRegex,
                _hrefAttributeRegex,
                source => source.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)));
    
            return sources;
        }

        private List<Uri> ExtractPageSources(Page page)
        {
            return ExtractSources(
                page,
                _aTagRegex,
                _hrefAttributeRegex,
                delegate(string source)
                    {
                        foreach (string excludingUriPath in this._excludingUriPaths)
                        {
                            if (source.StartsWith(excludingUriPath, StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }

                        return true;
                    });
        }

        public void QueueSource(Uri source)
        {
            lock (_requests)
            {
                if (_blockedSources.Contains(source.OriginalString.ToLower()))
                {
                    Tracer.WriteHint("Skipping add of source '{0}' because it is already scanned.", source);
                    return;
                }

                _requests.Add(new PageRequest(source));
                _blockedSources.Add(source.OriginalString.ToLower());
                Tracer.WriteHint("Added manual source '{0}'.", source);
            }
        }

        private void ProcessSources()
        {
            while (true)
            {
                var requests = new List<PageRequest>();
                lock (_requests)
                {
                    int range = 25;
                    if (range > _requests.Count)
                    {
                        range = _requests.Count;
                    }

                    requests.AddRange(_requests.GetRange(0, range));
                    _requests.RemoveRange(0, range);
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

                        List<Uri> itemSources = ExtractItemSources(page);
                        Tracer.WriteVerbose("Extraced {0} items sources from page '{1}'.", itemSources.Count, page.Source);

                        List<Uri> subPageSources = ExtractPageSources(page);
                        
                        List<PageRequest> subPageRequests =
                            subPageSources.ConvertAll(
                                delegate(Uri source)
                                    {
                                        var subPageRequest = new PageRequest(source);
                                        subPageRequest.Hops.AddRange(request.Hops);
                                        subPageRequest.Hops.Add(request.Source);
                                        
                                        return subPageRequest;
                                    });

                        if (NewItemSourcesAvailable != null)
                        {
                            NewItemSourcesAvailable(itemSources);
                        }

                        lock (_requests)
                        {
                            foreach (PageRequest subPageRequest in subPageRequests)
                            {
                            	string sourceText = subPageRequest.Source.OriginalString.ToLower();
                                if (_blockedSources.Contains(sourceText))
                                {
                                    continue;
                                }

                                _requests.Add(subPageRequest);
                                _blockedSources.Add(sourceText);
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
            foreach (Match tagMatch in tagRegex.Matches(page.Content))
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
