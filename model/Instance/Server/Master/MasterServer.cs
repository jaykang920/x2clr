using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Master
{
    public class MasterServer : AsyncTcpServer
    {
        public MasterServer()
            : base("MasterServer")
        {

        }
    }
}
