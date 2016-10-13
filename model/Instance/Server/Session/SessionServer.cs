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

            RegisterEvents();

            InitializeSendEvents();
            InitializeRecvEvents();

            Flow.SubscribeTo("Net");

            // Listen to Port passed in
        }

        void RegisterEvents()
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
