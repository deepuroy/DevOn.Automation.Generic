using System;
using System.Configuration;
using System.Net;
using Automation.Api.Utilities;

namespace Automation.Api.Tests
{
    public class BaseTests
    {
        /// <summary>
        /// Base Test which initializes testing context.
        /// </summary>
        public BaseTests()
        {
            ServiceHandler.BaseUrl = new Uri(ConfigurationManager.AppSettings["Url"]);
            var proxyDomain = ConfigurationManager.AppSettings["ProxyDomain"].ToUpper();
            var proxyPort = ConfigurationManager.AppSettings["ProxyPort"].ToUpper();
            var proxyUrl = string.Format("{0}:{1}", proxyDomain, proxyPort);
            ServiceHandler.Proxy = new WebProxy(proxyUrl);
            WebHeaderCollection BaseHeaders = new WebHeaderCollection();
            ServiceHandler.RequestHeaders = new WebHeaderCollection();
        }
    }
}