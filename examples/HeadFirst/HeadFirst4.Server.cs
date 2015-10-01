using System;

using x2;

namespace x2.Examples.HeadFirst
{
    class HeadFirst4Server
    {
        class CapitalizerServer : AsyncUdpLink
        {
            public CapitalizerServer()
                : base("CapitalizerServer")
            {
            }

            protected override void Setup()
            {
                base.Setup();
                EventFactory.Register<CapitalizeReq>();
                new CapitalizeResp().Bind(Send);
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