using DotNetTor.SocksPort;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Torrentinator.Library.RSS;
using Torrentinator.Library.Types;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace Torrentinator.Library.Services
{
    internal class TorrentService: IDisposable, ITorrentService
    {
        public class TorrentServiceOptions
        {
            public TorProxyOptions TorProxy { get; set; } = new TorProxyOptions();
            public BrowserOptions Browser { get; set; } = new BrowserOptions();
        }
        public class TorProxyOptions
        {
            public string Address { get; set; }
            public int SocksPort { get; set; }
            public int ControlPort { get; set; }
            public string ControlPassword { get; set; }
        }
        public class BrowserOptions
        {
            public string Accept { get; set; }
            public string AcceptEncoding { get; set; }
            public string AcceptCharset { get; set; }
            public string UserAgent { get; set; }
        }

        private string URL => "https://www.thepiratebay.org/rss/top100/207"; // Onion version "http://uj3wazyk5u4hnvtk.onion/rss/top100/207";
        private static HttpClient _HttpClient = new HttpClient();
        private TorrentServiceOptions Options { get; }

        public bool Connected { get; private set; } = false;
        public string ConnectionError { get; private set; }

        public string Address { get; private set; }

        public int SocksPort { get; private set; }

        public int ControlPort { get; private set; }

        public string TorIP { get; private set; }

        public string CurrentTorIP { get; private set; }

        public TorrentService(TorrentServiceOptions options)
        {
            this.Options = options;
            this.Address = Options.TorProxy.Address;
            this.SocksPort = Options.TorProxy.SocksPort;
            this.ControlPort = Options.TorProxy.ControlPort;
        }

        public void Dispose()
        {
            this.Disconnect();
        }

        public TorConnectResult Connect()
        {
            var requestUri = "http://icanhazip.com/";
            try
            {
                _HttpClient = new HttpClient(new SocksPortHandler(Options.TorProxy.Address, socksPort: Options.TorProxy.SocksPort));
                var message = _HttpClient.GetAsync(requestUri).Result;
                this.TorIP = message.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Your Tor IP: \t\t{this.TorIP}");

                // 3. Change Tor IP
                var controlPortClient = new DotNetTor.ControlPort.Client(Options.TorProxy.Address, controlPort: Options.TorProxy.ControlPort, password: Options.TorProxy.ControlPassword);
                controlPortClient.ChangeCircuitAsync().Wait();

                // 4. Get changed Tor IP
                message = _HttpClient.GetAsync(requestUri).Result;
                this.CurrentTorIP = message.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Your other Tor IP: \t{this.CurrentTorIP}");

                this.Connected = true;
                this.ConnectionError = string.Empty;

                return new TorConnectResult(this.CurrentTorIP, null);
            }
            catch (Exception ex)
            {                
                this.ConnectionError = ex.Message;
                if (ex.InnerException != null)
                    AppendInnerException(ex.InnerException);
                return new TorConnectResult(string.Empty, ex);
            }
        }

        private void AppendInnerException(Exception ex)
        {
            this.ConnectionError += "\r\n\t" + ex.Message;
            if (ex.InnerException != null)
                AppendInnerException(ex.InnerException);
        }
        public void Disconnect()
        {
            if (_HttpClient != null)            
                _HttpClient.CancelPendingRequests();
            _HttpClient = null;
        }

        private CookieContainer cookieContainer = null;

        private HttpWebResponse GetResponse(string url)
        {
            var success = false;

            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
                var requestHome = (HttpWebRequest)WebRequest.Create("https://www.thepiratebay.org/");

                requestHome.CookieContainer = cookieContainer;
                requestHome.Headers.Add(HttpRequestHeader.Accept, Options.Browser.Accept);
                requestHome.Headers.Add(HttpRequestHeader.AcceptEncoding, Options.Browser.AcceptEncoding);
                requestHome.Headers.Add(HttpRequestHeader.UserAgent, Options.Browser.UserAgent);
                requestHome.Headers.Add(HttpRequestHeader.AcceptCharset, Options.Browser.AcceptCharset);

                var responseHome = (HttpWebResponse)requestHome.GetResponse();
                success = (responseHome.StatusCode == HttpStatusCode.OK);

                responseHome.Close();
            }
            else
            {
                success = true;
            }

            if (success)
            {
                // now that we have cookies, change the request and get the stupid feed
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = cookieContainer;
                request.Headers.Add(HttpRequestHeader.Accept, Options.Browser.Accept);
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, Options.Browser.AcceptEncoding);
                request.Headers.Add(HttpRequestHeader.UserAgent, Options.Browser.UserAgent);
                request.Headers.Add(HttpRequestHeader.AcceptCharset, Options.Browser.AcceptCharset);

                return (HttpWebResponse)request.GetResponse();
            }
            return null;
        }
        private async Task<string> GetWebPage(string url)
        {
            var response = GetResponse(url);
            var ret = string.Empty;

            if ((response != null) && (response.StatusCode == HttpStatusCode.OK))
            {
                using (var decompressedStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                using (var sr = new StreamReader(decompressedStream, Encoding.UTF8))                
                    ret = await sr.ReadToEndAsync();                

                response.Close();
            }

            return ret;
        }

        public async Task<IEnumerable<TorrentRSSItem>> GetTorrentsFromRSS()
        {
            var links = new List<TorrentRSSItem>();
            var content = await GetWebPage(this.URL);

            if (!string.IsNullOrEmpty(content))
            {
                using (var stringReader = new StringReader(content))
                using (var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings() { Async = true, DtdProcessing = DtdProcessing.Parse }))
                {
                    var feedReader = new RssFeedReader(xmlReader, new TorrentRSSParser());

                    while (await feedReader.Read())
                    {
                        if (feedReader.ElementType == SyndicationElementType.Item)
                        {
                            var item = (TorrentRSSItem)await feedReader.ReadItem();
                            links.Add(item);
                        }
                    }
                }
            }

            return links;
        }

        public async Task<string> GetDescription(string torrentId)
        {
            var content = await GetWebPage(torrentId);
            var ret = (string)null;

            if (!string.IsNullOrEmpty(content))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(content);
                var divElt = doc.DocumentNode.QuerySelector("div.nfo");

                if (divElt != null)
                {
                    var pre = divElt.Children().Where(n => n.Name == "pre").FirstOrDefault();
                    if (pre != null)
                        return pre.InnerHtml;
                    return divElt.InnerHtml;
                }
            }

            return ret;
        }
    }
}
