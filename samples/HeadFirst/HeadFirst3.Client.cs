using System;

using x2;
using x2.Flows;
using x2.Links.SocketLink;

namespace x2.Samples.HeadFirst
{
    class HeadFirst3Client
    {
        class OutputCase : Case
        {
            protected override void SetUp()
            {
                new CapitalizeResp().Bind(e => Console.WriteLine(e.Result));
            }
        }

        class CapitalizerClient : AsyncTcpClient
        {
            public CapitalizerClient()
                : base("CapitalizerClient")
            {
                //IncomingKeepaliveEnabled = true
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
            Log.Level = LogLevel.Debug;
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