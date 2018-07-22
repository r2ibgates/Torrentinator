using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Torrentinator.Library.Types
{
    public class Progress
    {
        public long Total { get; set; }
        public long Completed { get; set; }
        private double Percent
        {
            get
            {

                if ((this.Total == 0) || (this.Completed == 0))
                    return 0;
                return (((float)this.Completed) / ((float)this.Total));
            }
        }
        public double PercentComplete
        {
            get
            {
                return Math.Round(this.Percent * 100.0, 2);
            }
        }
        public string PercentFormatted
        {
            get
            {
                return this.Percent.ToString("P");
            }
        }

        public Progress() { }
        public Progress(long total)
        {
            this.Total = total;
        }

        public override string ToString()
        {
            return this.PercentFormatted;
        }
    }
}
