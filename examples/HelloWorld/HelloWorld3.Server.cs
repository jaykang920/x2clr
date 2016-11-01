using System;

using x2;

namespace x2.Examples.HelloWorld
{
    // TCP server
    class HelloWorld3Server
    {
        class HelloServer : AsyncTcpServer
        {
            public HelloServer()
                : base("HelloServer")
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
                EventFactory.Register<HelloReq>();
                new HelloResp().Bind(Send);
                Listen(6789);
            }
        }

        public static void Main()
        {
            Config.LogLevel = LogLevel.Trace;
            Log.Handler = (level, message) => Console.WriteLine(message);

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new HelloCase())
                    .Add(new HelloServer()));

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