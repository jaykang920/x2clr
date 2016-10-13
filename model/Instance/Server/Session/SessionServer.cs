using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Session
{
    /// <summary>
    /// Handles Login, Lobby events for Users
    /// </summary>
    public class SessionServer : AsyncTcpServer
    {
        public SessionServer()
            : base("session")
        {

        }


    }

}
