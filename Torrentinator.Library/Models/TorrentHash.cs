using System;
using System.Collections.Generic;
using System.Text;

namespace Torrentinator.Library.Models
{
    public class TorrentHash
    {
        public int PieceIndex { get; private set; }
        public bool HashPassed { get; private set; }

        internal TorrentHash(int index, bool passed)
        {
            this.PieceIndex = index;
            this.HashPassed = passed;
        }
    }
}
