using System;

using x2;

namespace x2.Examples.HelloWorld
{
    // Connect-on-demand client
    class HelloWorld5Client
    {
        class HelloClient : TcpClient
        {
            public HelloClient()
                : base("HelloClient")
            {
                DisconnectOnComplete = true;

                BufferTransform = new BufferTransformStack()
                    .Add(new BlockCipher());
            }

            protected override void Setup()
            {
                EventFactory.Register<HelloResp>();
                new HelloReq().Bind(ConnectAndRequest);

                RemoteHost = "127.0.0.1";
                RemotePort = 6789;
            }
        }

        class DebugCase : Case
        {
            protected override void Setup()
            {
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
                    .Add(new HelloClient()));

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
                        new HelloReq { Name = message }.Post();
                    }
                }
            }
        }
    }
}