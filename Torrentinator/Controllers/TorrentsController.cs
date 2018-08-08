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
        private IDataService DataService;

        public TorrentsController(ITorrentService torrentService, IDataService dataService)
        {
            this.TorrentService = torrentService;
            this.DataService = dataService;
        }

        public async Task<IActionResult> Index()
        {
            /*
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
            */
            await this.DataService.RefreshTorrents();

            var torrents = (await this.DataService.GetTorrents()).Select(TorrentViewModel.Create);
            return View(torrents);
        }
    }
}