using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Torrentinator.Models
{
    public class ActiveTorrentViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string JSId
        {
            get
            {
                return Regex.Replace(this.Id.ToLower(), @"([^\w])+", "_");
            }
        }
    }
}
