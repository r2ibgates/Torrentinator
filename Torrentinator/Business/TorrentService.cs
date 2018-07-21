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
using Torrentinator.Models;
using Torrentinator.RSS;

namespace Torrentinator.Business
{
    public class TorrentService: IDisposable
    {
        private string URL => "https://www.thepiratebay.org/rss/top100/207"; // Onion version "http://uj3wazyk5u4hnvtk.onion/rss/top100/207";
        public string ControlIPAddress => "127.0.0.1";
        public int SocksPortNumber => 9050;
        public int ControlPortNumber => 9051;
        public string ControlPassword => "Password1";
        private static HttpClient _HttpClient = new HttpClient();

        public void Dispose()
        {
            this.Disconnect();
        }

        public async Task<TorConnectResult> Connect()
        {
            var requestUri = "http://icanhazip.com/";
            try
            {
                _HttpClient = new HttpClient(new SocksPortHandler(ControlIPAddress, socksPort: SocksPortNumber));
                var message = await _HttpClient.GetAsync(requestUri);
                var content = await message.Content.ReadAsStringAsync();
                Console.WriteLine($"Your Tor IP: \t\t{content}");

                // 3. Change Tor IP
                var controlPortClient = new DotNetTor.ControlPort.Client(ControlIPAddress, controlPort: ControlPortNumber, password: ControlPassword);
                await controlPortClient.ChangeCircuitAsync();

                // 4. Get changed Tor IP
                message = await _HttpClient.GetAsync(requestUri);
                content = await message.Content.ReadAsStringAsync();
                Console.WriteLine($"Your other Tor IP: \t{content}");

                return new TorConnectResult(content, null);
            }
            catch (Exception ex)
            {
                return new TorConnectResult(string.Empty, ex);
            }
        }

        public void Disconnect()
        {
            if (_HttpClient != null)            
                _HttpClient.CancelPendingRequests();
            _HttpClient = null;
        }

        public async Task<IEnumerable<TorrentRSSItem>> GetTorrents()
        {
            var links = new List<TorrentRSSItem>();
            var cookieContainer = new CookieContainer();

            var requestHome = (HttpWebRequest)WebRequest.Create("https://www.thepiratebay.org/");
            requestHome.CookieContainer = cookieContainer;
            requestHome.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml");
            requestHome.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            requestHome.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            requestHome.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1");

            var responseHome = (HttpWebResponse)requestHome.GetResponse();
            var success = (responseHome.StatusCode == HttpStatusCode.OK);

            responseHome.Close();
            if (success)
            {
                // now that we have cookies, change the request and get the stupid feed
                var request = (HttpWebRequest)WebRequest.Create(this.URL);
                request.CookieContainer = cookieContainer;
                request.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml");
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                request.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1");

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var decompressedStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                        using (var sr = new StreamReader(decompressedStream, Encoding.UTF8))
                        {
                            var content = await sr.ReadToEndAsync();

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
                    }
                }
            }

            return links;
        }
    }
}
