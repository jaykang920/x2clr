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
    /// 
    /// Matching Rule: 
    ///   - Supposing a two player board game. 
    ///   - Matches instantly with a Bot and matching is finished. Join process continues.
    ///   - Put the instance in candidateQueue 
    ///   - When a new match request arrives, use candidateQueue to match to the human player. 
    ///   - The new joined player is in waiting list watching the game with a bot.
    ///   - When a human player leaves, 
    /// </summary>
    public class MatchCase : Case
    {
        /// <summary>
        /// InstanRunner Entry to select from
        /// </summary>
        class InstanceRunnerEntry
        {
            public int ServerId;
            public int RunnerId;
            public int InstanceCount;
            public bool Up;
        }

        class InstanceEntry
        {
            public class Member
            {
                public string Account;
                public bool Waiting;
            }

            public int Id;
            public List<Member> Members = new List<Member>();
            public Events.InstanceStatus Status;
        }

        List<InstanceRunnerEntry> runners;
        Dictionary<int, InstanceEntry> instances;
        Queue<InstanceEntry> matchQueue;

        public MatchCase()
        {
            instances = new Dictionary<int, InstanceEntry>();
            matchQueue = new Queue<InstanceEntry>();
        }

        public int Zone { get; private set; }

        public MatchCase(int zone)
        {
            Zone = zone; 
        }

        protected override void Setup()
        {
            base.Setup();

            new EventMatchReq { Zone = this.Zone }.Bind(OnMatchReq); 
        }

        void OnMatchReq(EventMatchReq req)
        {
            if ( matchQueue.Count > 0)
            {
                // Push a human player to the instance replacing the bot when a game restarts.

            }
            else
            {
                // Create a new instance with a bot player

                // Push the instance into the matchQueue
            }
        }
    }
}
