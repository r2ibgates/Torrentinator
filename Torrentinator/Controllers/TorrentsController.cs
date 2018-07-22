using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Torrentinator.Business;
using Torrentinator.Models;
using Torrentinator.Library.Services;

namespace Torrentinator.Controllers
{
    public class TorrentsController : Controller
    {
        private ITorrentService TorrentService;

        public TorrentsController(ITorrentService torrentService)
        {
            this.TorrentService = torrentService;
        }

        public async Task<IActionResult> Index()
        {
            var connect = await this.TorrentService.Connect();

            if (connect.Success)
            {
                var torrents = await this.TorrentService.GetTorrentsFromRSS();
                return View(torrents);
            }
            else
            {
                return View("Error", new ErrorViewModel()
                {
                    ErrorStack = connect.ErrorMessage
                });
            }
        }
    }
}