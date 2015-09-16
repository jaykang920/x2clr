using System;

using x2;
using x2.Flows;

namespace x2.Examples.HeadFirst
{
    class HeadFirst3Server
    {
        class CapitalizerServer : x2.Links.Sockets.AsyncTcpServer
        {
            public CapitalizerServer()
                : base("CapitalizerServer")
            {
                IncomingKeepaliveEnabled = true;
                OutgoingKeepaliveEnabled = true;
                MaxKeepaliveFailureCount = 1;
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
            Log.Level = LogLevel.Trace;
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