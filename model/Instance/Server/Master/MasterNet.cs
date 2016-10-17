using System.Reflection;
using x2;

namespace Server.Master
{
    public class MasterNet : AsyncTcpServer
    {
        Config config; 
        public MasterNet(Config cfg)
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
            // Send or Multicast when recieved
            new Event().Bind(Send);
        }
    }
}
