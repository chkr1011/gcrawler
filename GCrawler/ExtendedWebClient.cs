using System;
using System.Net;

namespace GCrawler
{
    internal sealed class ExtendedWebClient : WebClient
    {
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        public CookieContainer CookieContainer
        {
            get
            {
                return _cookieContainer;
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            var webRequest = request as HttpWebRequest;

            if (webRequest != null)
            {
                webRequest.CookieContainer = _cookieContainer;
            }

            return request;
        }
    }
}
