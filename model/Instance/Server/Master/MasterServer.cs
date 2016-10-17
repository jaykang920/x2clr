using System.Reflection;
using x2;

namespace Server.Master
{
    public class MasterServer : AsyncTcpServer
    {
        Config config; 
        public MasterServer(Config cfg)
            : base(cfg.Name)
        {
            config = cfg;
        }

        protected override void Setup()
        {
            base.Setup();

            InitializeFactoryEvents();

            InitializeBinds();

            Flow.SubscribeTo(ChannelNames.GetSlaveServerChannel());

            Listen(config.Port);
        }

        void InitializeFactoryEvents()
        {
            EventFactory.Register(Assembly.Load(config.EventAssembly));
        }
        
        void InitializeBinds()
        {
            // Bind to Send  
        }
    }
}
