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
    public class SessionNetServer : AsyncTcpServer
    {
        Config config;

        public SessionNetServer(Config cfg)
            : base(cfg.Name)
        {
            config = cfg;
        }

        protected override void Setup()
        {
            base.Setup();

            InitializeFactoryEvents();
            InitializeBinds();

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
