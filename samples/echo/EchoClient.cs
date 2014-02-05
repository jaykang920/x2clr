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

    class EchoClient : ClientCase
    {
        public EchoClient()
            : base("CapitalizerClient")
        {
            AutoReconnect = true;
            RetryInterval = 1000;
        }

        protected override void OnSessionConnected(LinkSessionConnected e)
        {
            if (e.Result)
            {
                Console.WriteLine("Connected");

                Flow.Bind(new EchoReq(), Send);
            }
            else
            {
                Console.WriteLine("Connection Failed");
            }
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Flow.Unbind(new EchoReq(), Send);

            Console.WriteLine("Disconnected");
        }

        protected override void SetUp()
        {
            base.SetUp();

            Event.Register<EchoResp>();

            Console.WriteLine("Connecting...");

            Connect("127.0.0.1", 5678);
        }
    }

    class OutputFlow : SingleThreadedFlow
    {
        static void OnCapitalizeResp(EchoResp e)
        {
            Console.WriteLine(e.Message);
        }

        protected override void SetUp()
        {
            Subscribe(new EchoResp(), OnCapitalizeResp);
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
                .Attach(new OutputFlow().Add(new EchoClient()));

            Flow.StartAll();

            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }

                var e = new EchoReq {
                    Message = message
                };
                Hub.Get().Post(e);
            }

            Flow.StopAll();
        }
    }
}
