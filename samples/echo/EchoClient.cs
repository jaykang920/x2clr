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
        string message = new String('x', 256);

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

                Flow.Bind(new EchoReq(), Send);

                Bind(new TimeoutEvent { Key = session }, OnTimeout);
                TimeFlow.Default.ReserveRepetition(new TimeoutEvent { Key = session }, new TimeSpan(0, 0, 1));

                Bind(new EchoResp(), OnEchoResp);
                Flow.Post(new EchoReq {
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
            Unbind(new TimeoutEvent { Key = session }, OnTimeout);
            TimeFlow.Default.CancelRepetition(new TimeoutEvent { Key = session });

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

        void OnTimeout(TimeoutEvent e)
        {
            LinkSession.Diagnostics diag = session.Diag;

            Console.WriteLine("Rx = {0} Tx = {1}", diag.BytesReceived, diag.BytesSent);

            diag.ResetBytesReceived();
            diag.ResetBytesSent();
        }

        void OnEchoResp(EchoResp e)
        {
            Flow.Post(new EchoReq {
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
                .Attach(new SingleThreadedFlow().Add(new EchoClient()))
                .Attach(TimeFlow.Create());

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
