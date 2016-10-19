using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;
using Events.Instance;

namespace Server.Game
{
    /// <summary>
    /// Dispatch to InstanceRunner Channel. That's all.
    /// Channel name is prefix_id 
    /// InstanceRunner id must be from 1 to the number of InstanceRunners
    /// </summary>
    public class InstanceCoordinator : Core.ChannelCase
    {
        string prefix;
        int runnerCount;
        Random rand;

        public InstanceCoordinator(string prefix, int runnerCount)
        {
            this.prefix = prefix;
            this.runnerCount = runnerCount;
            this.rand = new Random();
        }

        protected override void Setup()
        {
            base.Setup();

            // Only events not posted
            new EventInstanceBase() { Posted = false }.Bind(OnPostToRunner);
        }

        void OnPostToRunner(EventInstanceBase evt)
        {
            evt.Posted = true; 

            if ( evt.RunnerId == 0)
            {
                evt.RunnerId = rand.Next() % runnerCount + 1;
            }

            evt._Channel = string.Format("{0}_{1}", prefix, evt.RunnerId);
            evt.Post();
        }
    }
}
