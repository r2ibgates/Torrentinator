using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Torrentinator.Models
{
    public class HomeViewModel
    {
        public bool Connected { get; set; }
        public string ConnectionError { get; set; }
        public string Address { get; set; }

        public int SocksPort { get; set; }

        public int ControlPort { get; set; }

        public string TorIP { get; set; }

        public string CurrentTorIP { get; set; }
    }
}
