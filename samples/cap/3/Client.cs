using System;
using System.Net;
using System.Text;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Links;

namespace x2.Samples.Capitalizer
{
    class CapitalizerClient : TcpClient
    {
        TcpLink.Session session;

        void Send(Event e)
        {
            session.Send(this, e);
        }

        protected override void OnSessionConnected(LinkSessionConnected e)
        {
            if (e.Result)
            {
                Console.WriteLine("Connected");

                session = (TcpLink.Session)e.Context;

                Flow.Bind(CapitalizeReq.New(), Send);
            }
            else
            {
                Console.WriteLine("Connection Failed");

                System.Threading.Thread.Sleep(1000);

                Reconnect((EndPoint)e.Context);
            }
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Flow.Unbind(CapitalizeReq.New(), Send);

            Console.WriteLine("Disconnected");

            Close();
            socket = null;
            Connect("127.0.0.1", 5678);
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
            Subscribe(CapitalizeResp.New(), OnCapitalizeResp);
        }
    }

    class ClientProgram
    {
        static void Main(string[] args)
        {
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

                var e = CapitalizeReq.New();
                e.Message = message;
                Hub.Get().Post(e);
            }

            Flow.StopAll();
        }
    }
}
