using System.Reflection;
using x2;

namespace Server.Master
{
    public class MasterNetServer : Core.NetServer
    {
        Config config; 
        public MasterNetServer(Config cfg)
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
            // Send or Multicast when recieved
            new Event().Bind(Send);
        }
    }
}
