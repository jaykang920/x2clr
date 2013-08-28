using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Links;

namespace x2.Samples.Capitalizer
{
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

    class CapitalizerServer : TcpServer
    {
        new class Session
        {
            public Link Link { get; set; }
            public Link.Session LinkSession { get; set; }

            public void OnConnect()
            {
                var e = CapitalizeResp.New();
                e.SessionHandle = LinkSession.Handle.ToInt64();
                Flow.Bind(e, OnCapitalizeResp);
            }

            public void OnDisconnect()
            {
                var e = CapitalizeResp.New();
                e.SessionHandle = LinkSession.Handle.ToInt64();
                Flow.Unbind(e, OnCapitalizeResp);
            }

            void OnCapitalizeResp(CapitalizeResp e)
            {
                LinkSession.Send(Link, e);
            }
        }

        private readonly IDictionary<IntPtr, Session> sessions;

        public CapitalizerServer()
        {
            sessions = new Dictionary<IntPtr, Session>();
        }

        protected override void OnSessionConnected(LinkSessionConnected e)
        {
            if (e.Result == false)
            {
                return;
            }

            var linkSession = (Link.Session)e.Context;
            var session = new Session { Link = this, LinkSession = linkSession };
            sessions.Add(linkSession.Handle, session);
            session.OnConnect();

            Console.WriteLine("Accepted socket handle {0}", linkSession.Handle);
        }

        protected override void OnSessionDisconnected(LinkSessionDisconnected e)
        {
            Console.WriteLine("Disconnected");

            Link.Session linkSession = (Link.Session)e.Context;
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
