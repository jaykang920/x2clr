using System;
using System.Net;
using System.Text;
using System.Threading;

using x2;
using x2.Events;
using x2.Flows;
using x2.Links.SocketLink;

namespace x2.Samples.Capitalizer
{
    using ClientCase = x2.Links.SocketLink.AsyncTcpClient;

    class CapitalizerClient : ClientCase
    {
        public CapitalizerClient()
            : base("CapitalizerClient")
        {
            AutoReconnect = true;
            RetryInterval = 1000;

            BufferTransform = new BufferTransformStack()
                .Add(new x2.Transforms.Cipher())
                .Add(new x2.Transforms.Inverse());
        }

        protected override void OnSessionConnected(LinkSessionConnected e)
        {
            if (e.Result)
            {
                Console.WriteLine("Connected");

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

        protected override void SetUp()
        {
            base.SetUp();

            Event.Register<CapitalizeResp>();

            Console.WriteLine("Connecting...");

            Connect("127.0.0.1", 5678);
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

            Hub.Instance
                .Attach(new OutputFlow().Add(new CapitalizerClient()));

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

                    var e = new CapitalizeReq
                    {
                        Message = message
                    };
                    Hub.Post(e);
                }
            }
        }
    }
}
