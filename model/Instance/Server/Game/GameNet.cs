using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Game
{
    public class GameNet : AsyncTcpServer
    {
        public GameNet()
            : base("GameServer")
        {

        }
    }
}
