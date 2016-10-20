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
    public class InstanceCoordinator : Case
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
            new EventInstanceBase() { RunnerId = 0 }.Bind(OnPostToRunner);
        }

        void OnPostToRunner(EventInstanceBase evt)
        {
            if (evt.RunnerId == 0)
            {
                evt.RunnerId = rand.Next() % runnerCount + 1;
            }

            // Post with a RunnerId to a InstanceRunner
            evt.Post();
        }
    }
}
