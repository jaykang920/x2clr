using System;

using x2;

namespace x2.Examples.HeadFirst
{
    class HeadFirst3Client
    {
        class CapitalizerClient : TcpClient
        {
            public CapitalizerClient()
                : base("CapitalizerClient")
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
                EventFactory.Register<CapitalizeResp>();
                new CapitalizeReq().Bind(Send);
                Connect("127.0.0.1", 6789);
            }
        }

        public static void Main()
        {
            Config.LogLevel = LogLevel.Trace;
            Log.Handler = (level, message) => Console.WriteLine(message);

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new OutputCase())
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