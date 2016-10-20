using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Game
{
    /// <summary>
    /// 
    /// </summary>
    public class MasterClient : AsyncTcpClient
    {
        string ip;
        int port;

        public MasterClient(string name, string ip, int port)
            : base(name)
        {
            this.ip = ip;
            this.port = port; 
        }

        protected override void Setup()
        {
            base.Setup();

            Connect(ip, port);

        }
    }
}
