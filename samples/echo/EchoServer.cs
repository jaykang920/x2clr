using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Links.SocketLink;

namespace x2.Samples.Echo
{
    using ServerCase = x2.Links.SocketLink.AsyncTcpServer;

    class EchoCase : Case
    {
        static void OnEchoReq(EchoReq req)
        {
            var resp = new EchoResp {
                _Handle = req._Handle,
                Message = req.Message
            };
            Flow.Post(resp);
        }

        protected override void SetUp()
        {
            Bind(new EchoReq(), OnEchoReq);
        }
    }

    class EchoServerCase : ServerCase
    {
        class Session
        {
            public LinkSession LinkSession { get; set; }

            public void OnConnect()
            {
                var e = new EchoResp {
                    _Handle = LinkSession.Handle
                };
                Flow.Bind(e, Send);
            }

            public void OnDisconnect()
            {
                var e = new EchoResp {
                    _Handle = LinkSession.Handle
                };
                Flow.Unbind(e, Send);
            }

            void Send(Event e)
            {
                LinkSession.Send(e);
            }
        }

        private readonly IDictionary<IntPtr, Session> sessions;

        public EchoServerCase()
            : base("EchoServer")
        {
            sessions = new Dictionary<IntPtr, Session>();
        }

        protected override void OnSessionConnected(LinkSessionConnected e)
        {
            if (e.Result == false)
            {
                return;
            }

            var linkSession = (LinkSession)e.Context;
            var session = new Session { LinkSession = linkSession };
            sessions.Add(linkSession.Handle, session);
            session.OnConnect();

            Console.WriteLine("Accepted socket handle {0}", linkSession.Handle);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Console.WriteLine("Disconnected");

            LinkSession linkSession = (LinkSession)e.Context;
            Session session;
            if (sessions.TryGetValue(linkSession.Handle, out session) == false)
            {
                return;
            }
            session.OnDisconnect();
            sessions.Remove(linkSession.Handle);
        }

        protected override void SetUp()
        {
            base.SetUp();

            Event.Register<EchoReq>();

            Console.WriteLine("Listening on 5678...");

            Listen(5678);
        }
    }

    class ServerProgram
    {
        static void Main(string[] args)
        {
            x2.Log.Handler = (level, message) => {
                Console.WriteLine("[x2] {0}", message);
            };
            x2.Log.Level = x2.LogLevel.Warning;

            Hub.Instance
                .Attach(new SingleThreadedFlow()
                    .Add(new EchoCase())
                    .Add(new EchoServerCase()));

            using (var flows = new Hub.Flows())
            {
                flows.Start();

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
