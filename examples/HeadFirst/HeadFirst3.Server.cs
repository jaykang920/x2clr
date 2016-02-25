using System;

using x2;

namespace x2.Examples.HeadFirst
{
    // TCP server
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
                SessionRecoveryEnabled = true;
                ///*
                BufferTransform = new BufferTransformStack()
                    .Add(new BlockCipher())
                    .Add(new Inverse());
                //*/
            }

            protected override void Setup()
            {
                base.Setup();
                EventFactory.Register<CapitalizeReq>();
                new CapitalizeResp().Bind(Send);
                Listen(6789);
            }
        }

        public static void Main()
        {
            Config.LogLevel = LogLevel.Trace;
            Log.Handler = (level, message) => Console.WriteLine(message);

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new CapitalizerCase())
                    .Add(new CapitalizerServer()));

            using (new Hub.Flows().Startup())
            {
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