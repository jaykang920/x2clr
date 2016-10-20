using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;
using Events.Instance;

namespace Server.Game
{
    public class InstanceRunner : Case
    {
        int serverId;
        int runnerId;

        public InstanceRunner(int serverId, int runnerId)
            : base()
        {
            this.serverId = serverId;
            this.runnerId = runnerId;
        }

        protected override void Setup()
        {
            base.Setup();

            // Get notified only when instance id is not known
            new EventJoinReq {  }.Bind(OnJoinReq);
        }

        void OnJoinReq(EventJoinReq req)
        {

        }
    }
}
