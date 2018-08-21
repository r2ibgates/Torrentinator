using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Torrentinator.Business;
using Torrentinator.Library.Services;
using Torrentinator.Models;

namespace Torrentinator.Controllers
{
    public class HomeController : Controller
    {
        private ITorrentService TorrentService;

        public HomeController(ITorrentService torrentService)
        {
            this.TorrentService = torrentService;
        }

        public IActionResult Index()
        {
            var model = new HomeViewModel()
            {
                Connected = this.TorrentService.Connected,
                ConnectionError = this.TorrentService.ConnectionError,
                Address = this.TorrentService.Address,
                ControlPort = this.TorrentService.ControlPort,
                SocksPort = this.TorrentService.SocksPort,
                TorIP = this.TorrentService.TorIP,
                CurrentTorIP = this.TorrentService.CurrentTorIP
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Reconnect()
        {
            this.TorrentService.Connect();
            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
