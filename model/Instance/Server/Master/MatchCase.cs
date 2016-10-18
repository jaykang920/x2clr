using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;
using Events.Instance;

namespace Server.Master
{
    /// <summary>
    /// Manages instance and matching. Join and leave from instance.
    /// </summary>
    public class MatchCase : Case
    {
        // Instance info, InstanceRunner info kept 

        public int World { get; private set; }

        public MatchCase(int world)
        {
            World = world; 
        }

        protected override void Setup()
        {
            base.Setup();

            // Event binding

            new EventMatchReq { World = this.World }.Bind(OnMatchReq); 
        }

        void OnMatchReq(EventMatchReq req)
        {
            // [1] Look for a proper instance

            // [2] if not found, then push into a matching queue

            // [3] Periodically do match
        }
    }
}
