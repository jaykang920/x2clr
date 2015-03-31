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
                new CapitalizeReq().Bind((req) => {
                    new CapitalizeResp {
                        Result = req.Message.ToUpper()
                    }.InResponseOf(req).Post();
                });
            }
        }

        class CapitalizerServer : AsyncTcpServer
        {
            public CapitalizerServer()
                : base("CapitalizerServer")
            {
                //OutgoingKeepaliveEnabled = true
            }

            protected override void SetUp()
            {
                base.SetUp();
                EventFactory.Register<CapitalizeReq>();
                new CapitalizeResp().Bind(Send);
                Listen(6789);
            }
        }

        public static void Main()
        {
            Log.Level = LogLevel.Trace;
            Log.Handler = (level, message) => { Console.WriteLine(message); };

            Hub.Instance
                .Attach(new SingleThreadedFlow()
                    .Add(new CapitalizerCase())
                    .Add(new CapitalizerServer()));

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