using System;

using x2;

namespace x2.Examples.HelloWorld
{
    // Unreliable UDP server
    class HelloWorld4Server
    {
        class HelloServer : AsyncUdpLink
        {
            public HelloServer()
                : base("HelloServer")
            {
            }

            protected override void Setup()
            {
                base.Setup();
                EventFactory.Register<HelloReq>();
                new HelloResp().Bind(Send);
                Bind(6789).Listen();
                AddEndPoint(new System.Net.IPEndPoint(
                    System.Net.IPAddress.Parse("127.0.0.1"), 6788));
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