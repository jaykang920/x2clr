using System;
using System.Reflection;

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
                Bind(new CapitalizeResp(), (e) => { Console.WriteLine(e.Result); });
            }
        }

        class CapitalizerClient : AsyncTcpClient
        {
            public CapitalizerClient() : base("CapitalizerClient") { }

            protected override void SetUp()
            {
                base.SetUp();
                EventFactory.Register<CapitalizeResp>();
                Bind(new CapitalizeReq(), Send);
                Connect("127.0.0.1", 6789);
            }
        }

        public static void Main()
        {
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
                        var req = new CapitalizeReq();
                        req.Message = message;
                        req.Post();
                    }
                }
            }
        }
    }
}