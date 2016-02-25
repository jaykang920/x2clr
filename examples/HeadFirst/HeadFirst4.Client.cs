using System;

using x2;

namespace x2.Examples.HeadFirst
{
    // Unreliable UDP client
    class HeadFirst4Client
    {
        class CapitalizerClient : UdpLink
        {
            public CapitalizerClient()
                : base("CapitalizerClient")
            {
            }

            protected override void Setup()
            {
                base.Setup();
                EventFactory.Register<CapitalizeResp>();
                new CapitalizeReq().Bind(Send);
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