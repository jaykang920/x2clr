using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Core
{
    /// <summary>
    /// Net client for other server 
    /// </summary>
    public class NetClient : AsyncTcpClient
    {
        ChannelFilter channelFilter; 

        public NetClient(string name)
            : base(name)
        {
            channelFilter = new ChannelFilter();            
        }

        public void AddFilter(int typeId, string channel)
        {
            channelFilter.Add(typeId, channel);
        }

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
