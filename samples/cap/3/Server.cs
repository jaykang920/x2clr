using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Links.SocketLink;

namespace x2.Samples.Capitalizer
{
    using ServerCase = x2.Links.SocketLink.AsyncTcpServer;
    using ServerFlow = x2.Links.SocketLink.AsyncTcpServerFlow;

    class CapitalizerFlow : SingleThreadedFlow
    {
        static void OnCapitalizeReq(CapitalizeReq req)
        {
            var resp = CapitalizeResp.New();
            resp.SessionHandle = req.SessionHandle;
            resp.Result = req.Message.ToUpper();
            Flow.PostAway(resp);
        }

        protected override void SetUp()
        {
            Subscribe(CapitalizeReq.New(), OnCapitalizeReq);
        }
    }

    class CapitalizerServer : ServerFlow
    {
        class Session
        {
            public LinkSession LinkSession { get; set; }

            public void OnConnect()
            {
                var e = CapitalizeResp.New();
                e.SessionHandle = LinkSession.Handle;
                Flow.Bind(e, Send);
            }

            public void OnDisconnect()
            {
                var e = CapitalizeResp.New();
                e.SessionHandle = LinkSession.Handle;
                Flow.Unbind(e, Send);
            }

            void Send(Event e)
            {
                LinkSession.Send(e);
            }
        }

        private readonly IDictionary<IntPtr, Session> sessions;

        public CapitalizerServer()
            : base("CapitalizerServer")
        {
            sessions = new Dictionary<IntPtr, Session>();

            link.BufferTransform = new x2.Transforms.Inverse();
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

        protected override void OnStart()
        {
            Console.WriteLine("Listening on 5678...");

            Listen(5678);
        }

        protected override void SetUp()
        {
            base.SetUp();

            Event.Register<CapitalizeReq>();
        }
    }

    class ServerProgram
    {
        static void Main(string[] args)
        {
            x2.Log.Handler = (level, message) => {
                Console.WriteLine("[x2] {0}", message);
            };
            x2.Log.Level = x2.LogLevel.All;

            Hub.Get()
                .Attach(new CapitalizerFlow())
                .Attach(new CapitalizerServer());

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
