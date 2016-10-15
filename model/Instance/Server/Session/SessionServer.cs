using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Session
{
    /// <summary>
    /// Listen
    /// </summary>
    public class SessionServer : AsyncTcpServer
    {
        public SessionServer()
            : base("SessionServer")
        {

        }

        protected override void Setup()
        {
            base.Setup();

            InitializeFactoryEvents();
            InitializeSendEvents();
            InitializeRecvEvents();

            Flow.SubscribeTo(ChannelNames.GetClientsChannel());

            // Listen to Port passed in
        }

        void InitializeFactoryEvents()
        {

        }

        void InitializeSendEvents()
        {
            // Bind to Send
        }

        void InitializeRecvEvents()
        {
            //   
        }
    }

}
