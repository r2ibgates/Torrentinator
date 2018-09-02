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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OctoTorrent.Client;
using OctoTorrent.Client.Encryption;
using OctoTorrent.BEncoding;
using Torrentinator.Library.Models;
using System.Diagnostics;
using OctoTorrent.Common;
using System.Threading;

namespace Torrentinator.Library.Services
{
    internal class TorrentService: IDisposable, ITorrentService
    {
        internal class Top10Listener : TraceListener
        {
            private readonly int _capacity;
            private readonly LinkedList<string> _traces;

            public Top10Listener(int capacity)
            {
                _capacity = capacity;
                _traces = new LinkedList<string>();
            }

            public override void Write(string message)
            {
                lock (_traces)
                    _traces.Last.Value += message;
            }

            public override void WriteLine(string message)
            {
                lock (_traces)
                {
                    if (_traces.Count >= _capacity)
                        _traces.RemoveFirst();

                    _traces.AddLast(message);
                }
            }

            public void ExportTo(TextWriter output)
            {
                lock (_traces)
                    foreach (var s in _traces)
                        output.WriteLine(s);
            }
        }

        public class TorrentServiceOptions
        {
            public TorProxyOptions TorProxy { get; set; } = new TorProxyOptions();
            public BrowserOptions Browser { get; set; } = new BrowserOptions();
            public ClientOptions Client { get; set; } = new ClientOptions();
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
        public class ClientOptions
        {
            public string Path { get; set; }
            public int UploadSlots { get; set; } = 4;
            public int MaxConnections { get; set; } = 60;
            public bool InitialSeedingEnabled { get; set; } = false;
            public int HalfOpenConnections { get; set; } = 5;
            public int MaxDownloadSpeed { get; set; } = 0;
            public int MaxUploadSpeed { get; set; } = 0;
            public EncryptionTypes AllowedEncryption { get; set; } = EncryptionTypes.All;
            public string FastResumePath { get; set; }
            public int ListenPort { get; set; } = 16589;
        }

        private ILogger Logger { get; }
        private string URL => "https://www.thepiratebay.org/rss/top100/207"; // Onion version "http://uj3wazyk5u4hnvtk.onion/rss/top100/207";
        private static HttpClient _HttpClient = new HttpClient();
        public EngineSettings TorrentEngineSettings { get; private set; }
        public TorrentSettings TorrentSettings { get; private set; }
        public ClientEngine TorrentClient { get; private set; }
        public BEncodedDictionary FastResume { get; private set; }
        public List<TorrentManager> Managers { get; } = new List<TorrentManager>();

        public TorrentServiceOptions Options { get; private set; }

        public bool Connected { get; private set; } = false;
        public string ConnectionError { get; private set; }

        public string Address { get; private set; }

        public int SocksPort { get; private set; }

        public int ControlPort { get; private set; }

        public string TorIP { get; private set; }

        public string CurrentTorIP { get; private set; }

        public TorrentService(TorrentServiceOptions options, ILoggerFactory logger)
        {
            this.Options = options;
            this.Address = Options.TorProxy.Address;
            this.SocksPort = Options.TorProxy.SocksPort;
            this.ControlPort = Options.TorProxy.ControlPort;
            this.Logger = logger.CreateLogger(this.GetType().FullName);
            this.TorrentEngineSettings = new EngineSettings(
                defaultSavePath: Options.Client.Path,
                globalMaxConnections: Options.Client.MaxConnections,
                globalHalfOpenConnections: Options.Client.HalfOpenConnections,
                globalMaxDownloadSpeed: Options.Client.MaxDownloadSpeed,
                globalMaxUploadSpeed: Options.Client.MaxUploadSpeed,
                allowedEncryption: Options.Client.AllowedEncryption,
                fastResumePath: Path.Combine(Options.Client.Path, "fastresume.data"));
            this.TorrentSettings = new TorrentSettings(
                Options.Client.UploadSlots,
                Options.Client.MaxConnections,
                Options.Client.MaxDownloadSpeed,
                Options.Client.MaxUploadSpeed,
                Options.Client.InitialSeedingEnabled);

            if (!Directory.Exists(Options.Client.Path))
                Directory.CreateDirectory(Options.Client.Path);
            if (!Directory.Exists(Path.Combine(Options.Client.Path, "Torrents")))
                Directory.CreateDirectory(Path.Combine(Options.Client.Path, "Torrents"));

            this.TorrentClient = new ClientEngine(this.TorrentEngineSettings);
            this.TorrentClient.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, Options.Client.ListenPort));           
            
            try
            {
                this.FastResume = BEncodedValue.Decode<BEncodedDictionary>(File.ReadAllBytes(Path.Combine(Options.Client.Path, "fastresume.data")));
            }
            catch
            {
                this.FastResume = new BEncodedDictionary();
            }
        }
        public void Dispose()
        {
            if (MonitorThread != null)
            {
                MonitorThread.Abort();
            }
            this.Disconnect();
        }

        public TorConnectResult Connect()
        {
            var requestUri = "http://icanhazip.com/";
            try
            {
                _HttpClient = new HttpClient(new SocksPortHandler(Options.TorProxy.Address, socksPort: Options.TorProxy.SocksPort));
                var message = _HttpClient.GetAsync(requestUri).Result;
                this.TorIP = message.Content.ReadAsStringAsync().Result.Trim('\n').Trim('\r');
                Logger.LogInformation("Your Tor IP: {0}", this.TorIP);

                // 3. Change Tor IP
                var controlPortClient = new DotNetTor.ControlPort.Client(Options.TorProxy.Address, controlPort: Options.TorProxy.ControlPort, password: Options.TorProxy.ControlPassword);
                controlPortClient.ChangeCircuitAsync().Wait();

                // 4. Get changed Tor IP
                message = _HttpClient.GetAsync(requestUri).Result;
                this.CurrentTorIP = message.Content.ReadAsStringAsync().Result.Trim('\n').Trim('\r');
                Logger.LogInformation("Your other Tor IP: {0}", this.CurrentTorIP);

                this.Connected = true;
                this.ConnectionError = string.Empty;

                return new TorConnectResult(this.CurrentTorIP, null);
            }
            catch (Exception ex)
            {                
                this.ConnectionError = ex.Message;
                if (ex.InnerException != null)
                    AppendInnerException(ex.InnerException);
                Logger.LogError(this.ConnectionError, $"Error while connecting to TOR at {this.Address}:{this.SocksPort}");

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
        static Top10Listener _listener;			// This is a subclass of TraceListener which remembers the last 20 statements sent to it

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

        public event EventHandler<TorrentHash> PieceHashed;
        public event EventHandler<Statistics> StatisticsUpdated;

        private static Thread MonitorThread = null;

        public void StartDownload(Models.Torrent torrent)
        {
            var manager = (TorrentManager)null;
            var t = (OctoTorrent.Common.Torrent)null;

            if (torrent.Url.StartsWith("magnet"))
                manager = new TorrentManager(new OctoTorrent.MagnetLink(torrent.Url), this.TorrentEngineSettings.SavePath, this.TorrentSettings, torrent.Title);
            else
            {
                var torrentPath = Path.Combine(Options.Client.Path, "Torrents", torrent.Title + ".torrent");
                using (var web = new WebClient())
                {
                    web.DownloadFile(torrent.Url, torrentPath);
                }
                t = OctoTorrent.Common.Torrent.Load(torrentPath);
                manager = new TorrentManager(t, this.TorrentEngineSettings.SavePath, this.TorrentSettings);
            }
            if (this.FastResume.ContainsKey(t.InfoHash.ToHex()))
                manager.LoadFastResume(new FastResume((BEncodedDictionary)this.FastResume[t.InfoHash.ToHex()]));
            this.TorrentClient.Register(manager);

            manager.PieceHashed += delegate (object o, PieceHashedEventArgs e) {
                this.PieceHashed?.Invoke(o, new TorrentHash(e.PieceIndex, e.HashPassed));
            };
            // Store the torrent manager in our list so we can access it later
            this.Managers.Add(manager);
            manager.Start();
            if (MonitorThread == null)
            {
                MonitorThread = new Thread(StartMonitor);
                MonitorThread.Start();
            }
        }

        private void StartMonitor()
        {
            var i = 0;
            var running = true;
            var stats = new Statistics();

            while (running)
            {
                if ((i++) % 10 == 0)
                {
                    running = this.Managers.Exists(delegate (TorrentManager m) { return m.State != TorrentState.Stopped; });
                    stats.Global.TotalDownloadSpeed = this.TorrentClient.TotalDownloadSpeed;
                    stats.Global.TotalUploadSpeed = this.TorrentClient.TotalUploadSpeed;
                    stats.Global.DiskReadSpeed = this.TorrentClient.DiskManager.ReadRate;
                    stats.Global.DiskWriteSpeed = this.TorrentClient.DiskManager.WriteRate;
                    stats.Global.DiskTotalRead = this.TorrentClient.DiskManager.TotalRead;
                    stats.Global.DiskTotalWritten = this.TorrentClient.DiskManager.TotalWritten;
                    stats.Global.TotalConnections = this.TorrentClient.ConnectionManager.OpenConnections;
                    stats.Torrents.Clear();

                    foreach (var manager in this.Managers)
                    {
                        var stat = new TorrentStatistics();

                        // stat.State = manager.State;
                        stat.Name = manager.Torrent.Name;
                        stat.Progress = manager.Progress;
                        stat.DownloadSpeed = manager.Monitor.DownloadSpeed;
                        stat.UploadSpeed = manager.Monitor.UploadSpeed;
                        stat.BytesDownloaded = manager.Monitor.DataBytesDownloaded;
                        stat.BytesUploaded = manager.Monitor.DataBytesUploaded;
                        stat.WarningMessage = manager.TrackerManager.CurrentTracker.WarningMessage;
                        stat.FailureMessage = manager.TrackerManager.CurrentTracker.FailureMessage;

                        // if (manager.PieceManager != null)
                        //    stat.CurrentRequests = manager.PieceManager.CurrentRequestCount();
                        foreach (var peerId in manager.GetPeers())
                            stat.Peers.Add(new Models.Peer()
                            {
                                ConnectionUri = peerId.Connection.Uri,
                                DownloadSpeed = peerId.Monitor.DownloadSpeed,
                                UploadSpeed = peerId.Monitor.UploadSpeed,
                                RequestingPieces = peerId.AmRequestingPiecesCount
                            });

                        if (manager.Torrent != null)
                            foreach (var file in manager.Torrent.Files)
                                stat.FilesCompleted.Add(file.Path, file.BitField.PercentComplete);

                        stats.Torrents.Add(manager.Torrent.Name, stat);
                    }
                    this.StatisticsUpdated?.Invoke(this, stats);
                }

                Thread.Sleep(500);
            }
        }
    }
}
