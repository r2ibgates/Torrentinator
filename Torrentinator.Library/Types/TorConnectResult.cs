using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Torrentinator.Library.Types
{
    public class TorConnectResult
    {
        public bool Success { get; }
        public string TorIP { get; }
        public string ErrorMessage { get; }

        internal TorConnectResult(string torIP, Exception ex)
        {
            this.TorIP = torIP;
            if (ex == null)
                this.Success = true;
            else
            {
                this.Success = false;
                this.ErrorMessage = ex.ToString();
            }
        }
    }
}
