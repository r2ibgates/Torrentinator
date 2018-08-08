﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Torrentinator.Library.Models
{
    public class Torrent
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTimeOffset Published { get; set; }
        public long Length { get; set; }
    }
}