using System;
using System.Collections.Generic;
using System.Threading;
using x2;
using NUnit.Framework;

namespace x2.Tests.Func
{
    [TestFixture]
    public class TestFuncTcpSimple
    {
        [Test]
        public void TestHelloExample()
        {
            // TcpServer / TcpClient on a single hub to test functionality.
            // Flow needs to subscribe for channel of Hub to emulate distributed communication.

            var serverFlow = new SingleThreadFlow();
            var clientFlow = new SingleThreadFlow();

            Hub.Instance
                .Attach(serverFlow)
                .Attach(clientFlow);

            serverFlow.Add(new ServerCase("server"));
            clientFlow.Add(new ClientCase("client"));

            Hub.Startup();

            // Channel is used to force to use Session 
            // instead of Hub post between Client and Server.

            serverFlow.SubscribeTo("server");
            clientFlow.SubscribeTo("client");

            // Make Hello works. 
            Thread.Sleep(1000); 

            Hub.Shutdown();
        }


        public class ServerCase : AsyncTcpServer
        {
            public ServerCase(string name)
                : base(name)
            {
            }

            protected override void Setup()
            {
                base.Setup();

                EventFactory.Register<HelloReq>();

                Preprocess += ChangeChannel; 

                new HelloResp().Bind(Send);

                new HelloReq().Bind(OnHello);

                Listen(6789);
            }

            protected void ChangeChannel(LinkSession session, Event e)
            {
                e._Channel = "server";
            }

            void OnHello(HelloReq req)
            {
                var resp = new HelloResp
                {
                    Result = String.Format("Hello, {0}!", req.Name)
                };

                resp._Channel = "server";

                resp.InResponseOf(req).Post();
            }
        }

        public class ClientCase : AsyncTcpClient
        {
            public int HelloCount { get; set; }

            public ClientCase(string name)
                : base(name)
            {
            }

            protected override void Setup()
            {
                base.Setup();

                EventFactory.Register<HelloResp>();

                Preprocess += ChangeChannel;

                new TimerFlowCaseEvent().Bind(OnTimerEvent);
                new FlowStart().Bind(OnFlowStart);
                new HelloReq().Bind(ConnectAndRequest);

                RemoteHost = "127.0.0.1";
                RemotePort = 6789;
            }

            protected void ChangeChannel(LinkSession session, Event e)
            {
                e._Channel = "client";
            }

            void OnTimerEvent(TimerFlowCaseEvent e)
            {
                HelloCount++;

                var hello = new HelloReq { Name = string.Format("Hello {0}", HelloCount) };

                hello._Channel = "client"; 

                hello.Post();
            }

            void OnFlowStart(FlowStart e)
            {
                TimeFlow.Default.ReserveRepetition(
                    new TimerFlowCaseEvent(),
                    TimeSpan.FromMilliseconds(10)
                );
            }
        }

    }
}
