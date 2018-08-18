﻿using DotNetTor.SocksPort;
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

        public TorrentService(TorrentServiceOptions options)
        {
            this.Options = options;
        }

        public void Dispose()
        {
            this.Disconnect();
        }

        public async Task<TorConnectResult> Connect()
        {
            var requestUri = "http://icanhazip.com/";
            try
            {
                _HttpClient = new HttpClient(new SocksPortHandler(Options.TorProxy.Address, socksPort: Options.TorProxy.SocksPort));
                var message = await _HttpClient.GetAsync(requestUri);
                var content = await message.Content.ReadAsStringAsync();
                Console.WriteLine($"Your Tor IP: \t\t{content}");

                // 3. Change Tor IP
                var controlPortClient = new DotNetTor.ControlPort.Client(Options.TorProxy.Address, controlPort: Options.TorProxy.ControlPort, password: Options.TorProxy.ControlPassword);
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

        public async Task<IEnumerable<TorrentRSSItem>> GetTorrentsFromRSS()
        {
            var links = new List<TorrentRSSItem>();
            var cookieContainer = new CookieContainer();

            var requestHome = (HttpWebRequest)WebRequest.Create("https://www.thepiratebay.org/");
            requestHome.CookieContainer = cookieContainer;
            requestHome.Headers.Add(HttpRequestHeader.Accept, Options.Browser.Accept);
            requestHome.Headers.Add(HttpRequestHeader.AcceptEncoding, Options.Browser.AcceptEncoding);
            requestHome.Headers.Add(HttpRequestHeader.UserAgent, Options.Browser.UserAgent);
            requestHome.Headers.Add(HttpRequestHeader.AcceptCharset, Options.Browser.AcceptCharset);

            var responseHome = (HttpWebResponse)requestHome.GetResponse();
            var success = (responseHome.StatusCode == HttpStatusCode.OK);

            responseHome.Close();
            if (success)
            {
                // now that we have cookies, change the request and get the stupid feed
                var request = (HttpWebRequest)WebRequest.Create(this.URL);
                request.CookieContainer = cookieContainer;
                request.Headers.Add(HttpRequestHeader.Accept, Options.Browser.Accept);
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, Options.Browser.AcceptEncoding);
                request.Headers.Add(HttpRequestHeader.UserAgent, Options.Browser.UserAgent);
                request.Headers.Add(HttpRequestHeader.AcceptCharset, Options.Browser.AcceptCharset);

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
