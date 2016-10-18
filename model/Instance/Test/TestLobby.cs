using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class TestLobby
    {
        /// <summary>
        /// Try to enter a channel instance finding or creating a proper one.
        /// </summary>
        [Test]
        public void TestInstanceChannelJoin()
        {
            // Use a simple broadcast with Event binding. 
            // Use ChannelFilter if required to partition flows


            // Event : 
            //   EventMatchReq 
            //  
            //      EventMasterMatchReq 
            //          EventInstanceCreateReq
            //          EventInstanceCreateResp
            // 
            //      EventMasterMatchResp
            // 
            //   EventMatchResp
            // 
            //   EventInstanceJoinReq
            //   EventInstanceJoinResp
            // 
        }

    }
}
