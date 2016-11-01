using System;

using x2;

namespace x2.Examples.HelloWorld
{
    // Connect-on-demaind server
    class HelloWorld5Server
    {
        class HelloServer : AsyncTcpServer
        {
            public HelloServer()
                : base("HelloServer")
            {
                BufferTransform = new BufferTransformStack()
                    .Add(new BlockCipher());
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