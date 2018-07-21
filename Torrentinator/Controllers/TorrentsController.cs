using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Torrentinator.Business;
using Torrentinator.Models;

namespace Torrentinator.Controllers
{
    public class TorrentsController : Controller
    {
        public async Task<IActionResult> Index()
        {
            using (var t = new TorrentService())
            {
                var connect = await t.Connect();

                if (connect.Success)
                {
                    var torrents = await t.GetTorrents();
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
}