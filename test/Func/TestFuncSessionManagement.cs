using System;
using System.Collections.Generic;
using System.Threading;
using x2;
using NUnit.Framework;

namespace x2.Tests.Func
{
    [TestFixture]
    public class TestFuncSessionManagement
    {
        /// <summary>
        /// Make a server topology built.
        /// Detect disconnect.
        /// Reconnect.
        /// </summary>
        [Test]
        public void TestClusterTopology()
        {
            // x2 provides tree topology, meaning servers connect chain forms a tree.

            // SessionServer.GameClientCase, SessionServer.MasterClientCase, GameServer.MasterClientCase

            // Following code emulates SessionServer.GameClientCase joins to GameServer.

            var serverFlow = new SingleThreadFlow();
            var clientFlow = new SingleThreadFlow();

            Hub.Instance
                .Attach(serverFlow)
                .Attach(clientFlow);

            var serverCase = new SampleServerCase("GameServer");
            var clientCase = new SampleClientCase("SessionClient");

            serverFlow.Add(serverCase);
            clientFlow.Add(clientCase);

            Hub.Startup();

            // Channel is used to force to use Session 
            // instead of Hub post between Client and Server.

            serverFlow.SubscribeTo("server");
            clientFlow.SubscribeTo("client");

            clientCase.Connect();

            while (clientCase.IsJoined == false )
            {
                Thread.Sleep(100);
            }

            Assert.IsTrue(clientCase.IsJoined);

            clientCase.Disconnect();

            Thread.Sleep(100);

            Hub.Shutdown();
        }
    }

    internal class SampleServerCase : AsyncTcpServer
    {
        public SampleServerCase(string name)
            : base(name)
        {
        }

        protected override void Setup()
        {
            base.Setup();

            EventFactory.Register<NodeJoinReq>();

            Preprocess += ChangeChannel;

            new NodeJoinReq().Bind(OnNodeJoinReq);
            new NodeJoinResp().Bind(Send);
            new LinkSessionConnected { LinkName = "GameServer" }.Bind(OnServerConnected);
            new LinkSessionDisconnected { LinkName = "GameServer" }.Bind(OnServerDisconnected);
            
            Listen(6789);
        }

        protected void ChangeChannel(LinkSession session, Event e)
        {
            e._Channel = "server";
        }

        void OnNodeJoinReq(NodeJoinReq req)
        {

            new NodeJoinResp { Name = "SessionClient", _Channel = "server"}
            .InResponseOf(req)
            .Post();

            // Remember the Handle for the name to handle disconnect
        }

        void OnServerConnected(LinkSessionConnected lsc)
        {

        }

        void OnServerDisconnected(LinkSessionDisconnected lsd)
        {

        }
    }

    internal class SampleClientCase : AsyncTcpClient
    {
        public bool IsJoined { get; set; }

        public SampleClientCase(string name)
            : base(name)
        {

        }

        protected override void Setup()
        {
            base.Setup();

            EventFactory.Register<NodeJoinResp>();

            Preprocess += ChangeChannel;

            new NodeJoinReq().Bind(Send);
            new NodeJoinResp().Bind(OnNodeJoinResp);

            new LinkSessionConnected { LinkName = "SessionClient" }.Bind(OnGameServerConnected);
            new LinkSessionDisconnected { LinkName = "SessionClient" }.Bind(OnGameServerDisconnected);

            RemoteHost = "127.0.0.1";
            RemotePort = 6789;
        }

        /// <summary>
        /// Called when received an Event. Changes Channel name to get notified.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="e"></param>
        protected void ChangeChannel(LinkSession session, Event e)
        {
            e._Channel = "client";
        }

        void OnGameServerConnected(LinkSessionConnected lsc)
        {
            new NodeJoinReq {
                Name = "SessionClient",
                _Channel = "client",
                _Handle = this.Session.Handle
            }.Post();
        }

        void OnGameServerDisconnected(LinkSessionDisconnected lsd)
        {
            // Left
        }

        void OnNodeJoinResp(NodeJoinResp resp)
        {
            IsJoined = true;
        }

    } 
}
