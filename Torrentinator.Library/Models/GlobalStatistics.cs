using System;
using System.Collections.Generic;
using System.Text;

namespace Torrentinator.Library.Models
{
    public class GlobalStatistics
    {
        public int TotalDownloadSpeed { get; set; }
        public int TotalUploadSpeed { get; set; }
        public int DiskReadSpeed { get; set; }
        public int DiskWriteSpeed { get; set; }
        public long DiskTotalRead { get; set; }
        public long DiskTotalWritten { get; set; }
        public int TotalConnections { get; set; }
    }
}
