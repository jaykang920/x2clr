using System;

using x2;
using x2.Flows;
using x2.Links.SocketLink;

namespace x2.Examples.HeadFirst
{
    class HeadFirst3Server
    {
        class CapitalizerServer : x2.Links.Sockets.AsyncTcpServer
        {
            public CapitalizerServer()
                : base("CapitalizerServer")
            {
                //OutgoingKeepaliveEnabled = true,
                ///*
                BufferTransform = new BufferTransformStack()
                    .Add(new x2.Transforms.BlockCipher())
                    .Add(new x2.Transforms.Inverse());
                //*/
            }

            protected override void SetUp()
            {
                base.SetUp();
                EventFactory.Register<CapitalizeReq>();
                new CapitalizeResp().Bind(Send);
                Listen(6789);
            }
        }

        public static void Main()
        {
            Log.Level = LogLevel.Debug;
            Log.Handler = (level, message) => Console.WriteLine(message);

            Hub.Instance
                .Attach(new SingleThreadedFlow()
                    .Add(new CapitalizerCase())
                    .Add(new CapitalizerServer()));

            using (var flows = new Hub.Flows())
            {
                flows.StartUp();

                while (true)
                {
                    string message = Console.ReadLine();
                    if (message == "quit")
                    {
                        break;
                    }
                }
            }
        }
    }
}