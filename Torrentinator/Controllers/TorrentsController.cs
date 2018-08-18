using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Torrentinator.Business;
using Torrentinator.Models;
using Torrentinator.Library.Services;
using Torrentinator.Library.Repositories;

namespace Torrentinator.Controllers
{
    public class TorrentsController : Controller
    {
        private ITorrentRepository TorrentRepository;

        public TorrentsController(ITorrentRepository torrentRepository)
        {
            this.TorrentRepository = torrentRepository;
        }

        public async Task<IActionResult> Index()
        {
            var torrents = (await this.TorrentRepository.GetTorrents()).Select(TorrentViewModel.Create);
            return View(torrents);
        }

        [HttpPost]
        public async Task<IActionResult> RefreshIndex()
        {
            await this.TorrentRepository.ImportTorrents();
            return Redirect("/Torrents");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await this.TorrentRepository.DeleteTorrent(id);
            return Redirect("/Torrents");
        }
    }
}