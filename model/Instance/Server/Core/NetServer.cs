using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Core
{
    /// <summary>
    /// Base for network potion of servers having ChannelFilter
    /// </summary>
    public class NetServer : AsyncTcpServer
    {
        ChannelFilter channelFilter;

        public NetServer(string name)
            : base(name)
        {
            channelFilter = new ChannelFilter();
        }

        /// <summary>
        /// Add typeId channel mapping
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="channel"></param>
        public void AddFilter(int typeId, string channel)
        {
            channelFilter.Add(typeId, channel);
        }

        /// <summary>
        /// Remove typeId channel mapping
        /// </summary>
        /// <param name="typeId"></param>
        public void RemoveFilter(int typeId)
        {
            channelFilter.Remove(typeId);
        }

        protected override void Setup()
        {
            base.Setup();

            Preprocess += Process;
        }

        void Process(LinkSession session, Event e)
        {
            channelFilter.Process(e);
        }
    }
}
