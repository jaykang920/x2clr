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

    class EchoClientCase : ClientCase
    {
        string message = new String('x', 4000);

        public EchoClientCase()
            : base("EchoClient")
        {
            AutoReconnect = true;
            RetryInterval = 1000;
        }

        protected override void OnSessionConnected(LinkSessionConnected e)
        {
            base.OnSessionConnected(e);

            if (e.Result)
            {
                Console.WriteLine("Connected");

                var linkSession = e.Context as LinkSession;

                Bind(new EchoResp { _Handle = linkSession.Handle }, OnEchoResp);

                Bind(new EchoReq(), Send);
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
            Console.WriteLine("{0}", e.Message);
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

            Hub.Instance
                .Attach(new SingleThreadedFlow()
                    .Add(new EchoClientCase()))
                .Attach(TimeFlow.Default);

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

                    var e = new EchoReq
                    {
                        Message = message
                    };
                    Hub.Post(e);
                }

                flows.ShutDown();
            }
        }
    }
}
