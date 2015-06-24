namespace GCrawler
{
    using System;
    using System.Net;

    internal sealed class ExtendedWebClient : WebClient
    {
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        public CookieContainer CookieContainer
        {
            get
            {
                return this._cookieContainer;
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            var webRequest = request as HttpWebRequest;

            if (webRequest != null)
            {
                webRequest.CookieContainer = this._cookieContainer;
            }

            return request;
        }
    }
}
