using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;

namespace Server.Core
{
    public class ChannelCase : Case
    {
        ChannelFilter channelFilter;

        public ChannelCase()
            : base()
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

        public void Post(Event e)
        {
            // Set channel if filter exists
            channelFilter.Process(e);

            // Then, Post() to the Hub
            e.Post();
        }
    }
}
