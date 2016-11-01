using System;

using x2;

namespace x2.Examples.SessionRecovery
{
    class SessionRecoveryClient
    {
        public static void Main()
        {
            Config.LogLevel = LogLevel.Debug;
            Log.Handler = (level, message) => Console.WriteLine(message);

            var client = new Client();

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(client))
                .Attach(new SingleThreadFlow()
                    .Add(new WatchDogCase())
                    .Add(new GeneratorCase()));

            using (new Hub.Flows().Startup())
            {
                while (true)
                {
                    string message = Console.ReadLine();
                    if (message == "quit")
                    {
                        break;
                    }
                    else
                    {
                        var tcpSession = (AbstractTcpSession)client.Session;
                        if ((object)tcpSession != null)
                        {
                            tcpSession.OnDisconnect();
                        }
                    }
                }
            }
        }
    }

    class Client : TcpClient
    {
        public Client()
            : base("Client")
        {
            SessionRecoveryEnabled = true;

            BufferTransform = new BufferTransformStack()
                .Add(new BlockCipher());
        }

        protected override void Setup()
        {
            EventFactory.Register<TestResp>();
            Bind(new TestReq(), Send);
            Connect("127.0.0.1", 6789);
        }

        protected override void OnSessionConnected(bool result, object context)
        {
            base.OnSessionConnected(result, context);

            if (result)
            {
                TimeFlow.Default.ReserveRepetition(new TimeoutEvent(),
                    new TimeSpan(0, 0, 0, 0, 2));
            }
        }

        protected override void OnSessionDisconnected(int handle, object context)
        {
            base.OnSessionDisconnected(handle, context);

            TimeFlow.Default.CancelRepetition(new TimeoutEvent());
        }
    }

    class WatchDogCase : Case
    {
        private long rxSerial = -1;
        private long txSerial = -1;

        protected override void Setup()
        {
            Bind(new TestReq(), OnTestReq);
            Bind(new TestResp(), OnTestResp);
        }

        void OnTestReq(TestReq e)
        {
            if (e.Serial != ++txSerial)
            {
                Console.WriteLine("ERROR tx expected {0} but {1}",
                    txSerial, e.Serial);
                Environment.Exit(1);
            }
        }

        void OnTestResp(TestResp e)
        {
            //Console.WriteLine("rs {0}", e.Serial);
            if (e.Serial != ++rxSerial)
            {
                Console.WriteLine("ERROR rx expected {0} but {1}",
                    rxSerial, e.Serial);
                Environment.Exit(1);
            }
        }
    }

    class GeneratorCase : Case
    {
        private long serial = 0;

        protected override void Setup()
        {
            Bind(new TimeoutEvent(), OnTimer);
        }

        void OnTimer(TimeoutEvent e)
        {
            new TestReq { Serial = serial++ }.Post();
        }
    }
}