using System;

using x2;

namespace x2.Examples.HelloWorld
{
    // TCP client
    class HelloWorld3Client
    {
        class HelloClient : TcpClient
        {
            public HelloClient()
                : base("HelloClient")
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
                EventFactory.Register<HelloResp>();
                new HelloReq().Bind(Send);
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