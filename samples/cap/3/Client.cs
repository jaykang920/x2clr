using System;
using System.Net;
using System.Text;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Links.AsyncTcpLink;

namespace x2.Samples.Capitalizer
{
    class CapitalizerClient : AsyncTcpClient
    {
        AsyncTcpLinkSession session;

        public CapitalizerClient()
            : base("CapitalizerClient")
        {
            AutoReconnect = true;
            RetryInterval = 1000;
        }

        void Send(Event e)
        {
            Console.WriteLine("Sending: {0}", e);

            session.Send(e);
        }

        protected override void OnSessionConnected(LinkSessionConnected e)
        {
            if (e.Result)
            {
                Console.WriteLine("Connected");

                session = (AsyncTcpLinkSession)e.Context;

                Flow.Bind(new CapitalizeReq(), Send);
            }
            else
            {
                Console.WriteLine("Connection Failed");
            }
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Flow.Unbind(new CapitalizeReq(), Send);

            Console.WriteLine("Disconnected");
        }

        protected override void OnStart()
        {
            Console.WriteLine("Connecting...");

            Connect("127.0.0.1", 5678);
        }

        protected override void SetUp()
        {
            base.SetUp();

            Event.Register<CapitalizeResp>();
        }
    }

    class OutputFlow : SingleThreadedFlow
    {
        static void OnCapitalizeResp(CapitalizeResp e)
        {
            Console.WriteLine(e.Result);
        }

        protected override void SetUp()
        {
            Subscribe(new CapitalizeResp(), OnCapitalizeResp);
        }
    }

    class ClientProgram
    {
        static void Main(string[] args)
        {
            x2.Log.Handler = (level, message) => {
                Console.WriteLine("[x2] {0}", message);
            };
            x2.Log.Level = x2.LogLevel.All;

            Hub.Get()
                .Attach(new CapitalizerClient())
                .Attach(new OutputFlow());

            Flow.StartAll();

            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }

                var e = new CapitalizeReq {
                    Message = message
                };
                Hub.Get().Post(e);
            }

            Flow.StopAll();
        }
    }
}
