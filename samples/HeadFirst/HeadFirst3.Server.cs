using System;
using System.Reflection;

using x2;
using x2.Flows;
using x2.Links.SocketLink;

namespace x2.Samples.HeadFirst
{
    class HeadFirst3Server
    {
        class CapitalizerCase : Case
        {
            protected override void SetUp()
            {
                Bind(new CapitalizeReq(), (req) => {
                    Console.WriteLine(req);
                    new CapitalizeResp { Result = req.Message.ToUpper() }.Post();
                });
            }
        }

        class CapitalizerServer : AsyncTcpServer
        {
            public CapitalizerServer() : base("CapitalizerServer") { }

            protected override void SetUp()
            {
                base.SetUp();

                Bind(new CapitalizeResp(), Broadcast);

                Listen(6789);
            }
        }

        public static void Main()
        {
            Hub.Instance
                .Attach(new SingleThreadedFlow()
                    .Add(new CapitalizerCase())
                    .Add(new CapitalizerServer()));

            Event.Register(Assembly.Load("HeadFirst"));

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
                }
            }
        }
    }
}