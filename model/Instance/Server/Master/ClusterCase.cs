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
    public class ClusterCase : Core.ChannelCase
    {
        class Entry
        {
            public int Id;
            public int Role;
            public string Ip;
            public int Port;
            public bool Up;
            public int Handle;

            public void FromServerStatus(ServerStatus status)
            {
                Id = status.Id;
                Role = status.Role;
                Ip = status.Ip;
                Port = status.Port;
                Up = status.Up;
            }

            public void ToServerStatus(ServerStatus status)
            {
                status.Id = Id;
                status.Role = Role;
                status.Ip = Ip;
                status.Port = Port;
                status.Up = Up;
            }
        }

        private Dictionary<int, Entry> dictionary;  // Id as Key
        private List<int> handles;

        public ClusterCase()
            : base()
        {
            dictionary = new Dictionary<int, Entry>();
            handles = new List<int>();
        }


        protected override void Setup()
        {
            base.Setup();

            new EventJoin().Bind(OnJoin);
            new EventLeave().Bind(OnLeave);
        }

        void OnJoin(EventJoin join)
        {
            Entry server;

            if ( !dictionary.TryGetValue(join.Id, out server))
            {
                server = new Master.ClusterCase.Entry();

                server.Id = join.Id;
                server.Role = join.Role;
                server.Ip = join.Ip;
                server.Port = join.Port; 
            }

            server.Handle = join._Handle;

            dictionary[join.Id] = server;
            dictionary[join.Id].Up = true;

            handles.Remove(join._Handle);
            handles.Add(join._Handle);

            PostList();
        }
     
        void OnLeave(EventLeave leave)
        {
            dictionary.Remove(leave.Id);
            handles.Remove(leave._Handle);

            PostList();
        }

        void PostList()
        {
            var lst = new List<ServerStatus>();

            foreach (var pair in dictionary)
            {
                var ss = new ServerStatus();
                pair.Value.ToServerStatus(ss);
                lst.Add(ss);
            }

            var resp = new EventServerList();
            resp.Servers = lst;
            resp.AddMulticast(handles);

            Post(resp);
        }
    }
}
