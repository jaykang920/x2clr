using System;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using x2;
using Server.Master;
using Events.Cluster;

namespace Test
{
    [TestFixture]
    public class TestDirectoryServer
    {
        [Test]
        public void TestEventFlow()
        {
            var serverFlow = new SingleThreadFlow();
            var slaveFlow = new SingleThreadFlow();

            Hub.Instance
                .Attach(serverFlow)
                .Attach(slaveFlow)
                .Attach(TimeFlow.Default);

            var dir = new DirectoryServer();
            var sim = new SimulatedServer();

            serverFlow.Add(dir);
            slaveFlow.Add(sim);

            Hub.Startup();

            // Channel Filter setup for tests
            serverFlow.SubscribeTo(Server.ChannelNames.GetMasterServerChannel());
            slaveFlow.SubscribeTo(Server.ChannelNames.GetSlaveServerChannel());

            dir.SetDownstreamChannel(Server.ChannelNames.GetSlaveServerChannel());

            sim.Join();

            while (sim.Servers == null)
            {
                Thread.Sleep(10);
            }

            Assert.IsTrue(sim.Servers != null);
            Assert.IsTrue(sim.Servers[0].Id == 1);

            Hub.Shutdown();
        }
    }

    class SimulatedServer : Case
    {
        public List<Events.Cluster.ServerStatus> Servers; 

        public void Join()
        {
            // Post Join
            Post(
                new Events.Cluster.EventJoin()
                {
                    Id = 1, 
                    Role = 2, 
                    Ip = "127.0.0.1", 
                    Port = 1234
                }
            );
        }

        protected override void Setup()
        {
            new EventServerList().Bind(OnServerList);
        }

        /// <summary>
        /// Use this to Post to Master channel
        /// </summary>
        /// <param name="e"></param>
        void Post(Event e)
        {
            e._Channel = Server.ChannelNames.GetMasterServerChannel();
            e.Post();
        }

        /// <summary>
        /// ServerList received
        /// </summary>
        /// <param name="e"></param>
        void OnServerList(EventServerList e)
        {
            Servers = e.Servers;
        }
    }
}
