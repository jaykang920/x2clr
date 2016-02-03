using System;
using System.Collections.Generic;

using x2;

namespace x2.Examples.SessionRecovery
{
    class SessionRecoveryServer
    {
        public static void Main()
        {
            Config.LogLevel = LogLevel.Info;
            Log.Handler = (level, message) => Console.WriteLine(message);

            var server = new Server();

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(server))
                .Attach(new SingleThreadFlow()
                    .Add(new WatchDogCase())
                    .Add(new EchoCase()));

            using (new Hub.Flows().Startup())
            {
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


    /// <summary>
    /// SessionServer 사용자 세션
    /// </summary>
    class Session : EventSink
    {
        public int Id { get; private set; }
        public LinkSession LinkSession { get; set; }

        public Session(int id)
        {
            Id = id;

            Bind(new TestResp { _Handle = Id }, Send);
        }

        public void Close(bool disconnect)
        {
            Dispose();

            if (LinkSession != null)
            {
                if (disconnect)
                {
                    LinkSession.Close();
                }
                else
                {
                    LinkSession = null;
                }
            }
        }

        void Send(Event e)
        {
            if (LinkSession == null)
            {
                return;
            }
            LinkSession.Send(e);
        }
    }

    class Server : AsyncTcpServer
    {
        private Dictionary<int, Session> clientSessions;

        public Server()
            : base("Server")
        {
            clientSessions = new Dictionary<int, Session>();

            SessionRecoveryEnabled = true;

            BufferTransform = new BufferTransformStack()
                .Add(new BlockCipher());
        }

        protected override void Setup()
        {
            base.Setup();
            EventFactory.Register<TestReq>();
            Listen(6789);
        }

        protected override void OnSessionConnected(bool result, object context)
        {
            base.OnSessionConnected(result, context);

            if (result)
            {
                var linkSession = (LinkSession)context;
                Session session = new Session(linkSession.Handle) {
                    LinkSession = linkSession
                };
                clientSessions.Add(session.Id, session);
            }
        }

        protected override void OnSessionDisconnected(int handle, object context)
        {
            base.OnSessionDisconnected(handle, context);

            clientSessions.Remove(handle);
        }

        protected override void OnSessionRecovered(int handle, object context)
        {
            base.OnSessionRecovered(handle, context);

            clientSessions[handle].LinkSession = (LinkSession)context;
        }
    }

    class WatchDogCase : Case
    {
        private long rxSerial = -1;
        private long txSerial = -1;

        protected override void Setup()
        {
            base.Setup();
            Bind(new TestReq(), OnTestReq);
            Bind(new TestResp(), OnTestResp);
        }

        void OnTestReq(TestReq e)
        {
            if (e.Serial != ++txSerial)
            {
                Console.WriteLine("ERROR tx exptected {0} but {1}",
                    txSerial, e.Serial);
                Environment.Exit(1);
            }
        }

        void OnTestResp(TestResp e)
        {
            if (e.Serial != ++rxSerial)
            {
                Console.WriteLine("ERROR rx exptected {0} but {1}",
                    rxSerial, e.Serial);
                Environment.Exit(1);
            }
        }
    }

    class EchoCase : Case
    {
        protected override void Setup()
        {
            base.Setup();

            Bind(new TestReq(), OnTestReq);
        }

        void OnTestReq(TestReq req)
        {
            new TestResp {
                Serial = req.Serial
            }.InResponseOf(req).Post();
        }
    }
}