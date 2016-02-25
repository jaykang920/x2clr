using System;

using x2;

namespace x2.Examples.HeadFirst
{
    // Connect-on-demand client
    class HeadFirst5Client
    {
        class CapitalizerClient : TcpClient
        {
            public CapitalizerClient()
                : base("CapitalizerClient")
            {
                DisconnectOnComplete = true;

                BufferTransform = new BufferTransformStack()
                    .Add(new BlockCipher());
            }

            protected override void Setup()
            {
                base.Setup();
                EventFactory.Register<CapitalizeResp>();
                new CapitalizeReq().Bind(ConnectAndRequest);

                RemoteHost = "127.0.0.1";
                RemotePort = 6789;
            }
        }

        class DebugCase : Case
        {
            protected override void Setup()
            {
                base.Setup();
                Bind(new Event(), OnEvent);
            }

            void OnEvent(Event e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Main()
        {
            Config.LogLevel = LogLevel.Trace;
            Log.Handler = (level, message) => Console.WriteLine(message);

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new OutputCase())
                    //.Add(new DebugCase())
                    .Add(new CapitalizerClient()));

            using (new Hub.Flows().Startup())
            {
                while (true)
                {
                    string message = Console.ReadLine();
                    if (message == "quit")
                    {
                        break;
                    }
                    else
                    {
                        new CapitalizeReq { Message = message }.Post();
                    }
                }
            }
        }
    }
}