using System;

using x2;

namespace x2.Examples.HeadFirst
{
    class HeadFirst3Server
    {
        class CapitalizerServer : AsyncTcpServer
        {
            public CapitalizerServer()
                : base("CapitalizerServer")
            {
                IncomingKeepaliveEnabled = true;
                OutgoingKeepaliveEnabled = true;
                MaxKeepaliveFailureCount = 1;
                ///*
                BufferTransform = new BufferTransformStack()
                    .Add(new BlockCipher())
                    .Add(new Inverse());
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
                .Attach(new SingleThreadFlow()
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