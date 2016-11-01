using System;

using x2;

namespace x2.Examples.HelloWorld
{
    // Unreliable UDP client
    class HelloWorld4Client
    {
        class HelloClient : UdpLink
        {
            public HelloClient()
                : base("HelloClient")
            {
            }

            protected override void Setup()
            {
                EventFactory.Register<HelloResp>();
                new HelloReq().Bind(Send);
                Bind(6788).Listen();
                AddEndPoint(new System.Net.IPEndPoint(
                    System.Net.IPAddress.Parse("127.0.0.1"), 6789));
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