using System;

using x2;

namespace x2.Examples.HeadFirst
{
    class HeadFirst3Client
    {
        class CapitalizerClient : AsyncTcpClient
        {
            public CapitalizerClient()
                : base("CapitalizerClient")
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
                EventFactory.Register<CapitalizeResp>();
                new CapitalizeReq().Bind(Send);
                Connect("127.0.0.1", 6789);
            }
        }

        public static void Main()
        {
            Log.Level = LogLevel.Trace;
            Log.Handler = (level, message) => Console.WriteLine(message);

            Hub.Instance
                .Attach(new SingleThreadedFlow()
                    .Add(new OutputCase())
                    .Add(new CapitalizerClient()));

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
                    else
                    {
                        new CapitalizeReq { Message = message }.Post();
                    }
                }
            }
        }
    }
}