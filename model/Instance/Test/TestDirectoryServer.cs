using System;
using System.Threading;
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


            Hub.Shutdown();
        }
    }

    class SimulatedServer : Case
    {
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
        }

        /// <summary>
        /// ServerList received
        /// </summary>
        /// <param name="e"></param>
        void OnServerList(EventServerList e)
        {

        }
    }
}
