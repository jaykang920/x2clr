using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using x2;

namespace Server.Session
{
    /// <summary>
    /// Listen
    /// </summary>
    public class SessionNet : AsyncTcpServer
    {
        Config config;

        public SessionNet(Config cfg)
            : base(cfg.Name)
        {
            config = cfg;
        }

        protected override void Setup()
        {
            base.Setup();

            InitializeFactoryEvents();
            InitializeBinds();

            Flow.SubscribeTo(ChannelNames.GetClientsChannel());

            Listen(config.Port);
        }

        void InitializeFactoryEvents()
        {
            EventFactory.Register(Assembly.Load(config.EventAssembly));
        }

        void InitializeBinds()
        {
            new Event().Bind(Send);
        }
    }
}
