using System;
using System.Net;
using System.Text;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Links.SocketLink;

namespace x2.Samples.Echo
{
    using ClientCase = x2.Links.SocketLink.AsyncTcpClient;
    using ClientFlow = x2.Links.SocketLink.AsyncTcpClientFlow;

    class EchoClient : ClientFlow
    {
        string message = new String('x', 4000);

        public EchoClient()
            : base("EchoClient")
        {
            AutoReconnect = true;
            RetryInterval = 1000;
        }

        protected override void OnSessionConnected(LinkSessionConnected e)
        {
            if (e.Result)
            {
                Console.WriteLine("Connected");

                var linkSession = e.Context as LinkSession;

                Bind(new EchoResp { SessionHandle = linkSession.Handle }, OnEchoResp);

                link.Send(new EchoReq {
                    Message = message
                });
            }
            else
            {
                Console.WriteLine("Connection Failed");
            }
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Console.WriteLine("Disconnected");
        }

        protected override void SetUp()
        {
            base.SetUp();

            Event.Register<EchoResp>();

            Console.WriteLine("Connecting...");

            Connect("127.0.0.1", 5678);
        }

        void OnEchoResp(EchoResp e)
        {
            link.Send(new EchoReq {
                Message = message
            });
        }
    }

    class ClientProgram
    {
        static void Main(string[] args)
        {
            x2.Log.Handler = (level, message) => {
                Console.WriteLine("[x2] {0}", message);
            };
            x2.Log.Level = x2.LogLevel.Warning;

            Hub.Get()
                .Attach(new EchoClient())
                .Attach(TimeFlow.Create());

            /*
            for (int i = 0; i < 2; ++i)
            {
                Hub.Get().Attach(new EchoClient());
            }
            */

            Flow.StartAll();

            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }
            }

            Flow.StopAll();
        }
    }
}
