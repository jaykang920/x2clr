using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;
using Events.Cluster;

namespace Server.Master
{
    /// <summary>
    /// Manages server configuration and instances.
    /// </summary>
    public class DirectoryServer : Case
    {
        class Entry
        {
            public int Id;
            public string Ip;
            public int Port;
            public bool Status;
            public int Handle;

            public void FromServerStatus(ServerStatus status)
            {
                Id = status.Id;
                Ip = status.Ip;
                Port = status.Port;
                Status = status.Status;
            }

            public void ToServerStatus(ServerStatus status)
            {
                status.Id = Id;
                status.Ip = Ip;
                status.Port = Port;
                status.Status = Status;
            }
        }

        private string downstreamChannel;
        private Dictionary<int, Entry> dictionary;  // Id as Key

        public DirectoryServer()
            : base()
        {
            dictionary = new Dictionary<int, Entry>();
        }


        /// <summary>
        /// Set Flow downstream channel to send to net or local processing
        /// </summary>
        /// <param name="channel"></param>
        public void SetDownstreamChannel(string channel)
        {
            downstreamChannel = channel;
        }

        protected override void Setup()
        {
            base.Setup();

            new EventJoin().Bind(OnJoin);
            new EventLeave().Bind(OnLeave);
        }

        void OnJoin(EventJoin join)
        {

        }
     
        void OnLeave(EventLeave leave)
        {

        }

        void PostList()
        {
            // Send the List to all servers.
        }

        void Post(Event e)
        {
            e._Channel = downstreamChannel;
        }
    }
}
