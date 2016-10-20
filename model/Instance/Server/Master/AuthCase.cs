using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;
using Events;
using Events.Login;

namespace Server.Master
{
    /// <summary>
    /// Authenticate and manages User location
    /// </summary>
    public class AuthCase : Case
    {
        class Entry
        {
            public string Account;
            public string Nick;
            public int Guid;
            public int Handle;         // From SessionServer
        }

        Dictionary<string, Entry> dic;
        // TODO: After DB processing, Dictionary<int, Entry> indexerGuid;

        public AuthCase()
        {
            dic = new Dictionary<string, Entry>();
        }

        protected override void Setup()
        {
            base.Setup();

            new EventMasterLoginReq().Bind(OnLoginReq);
            new EventMasterLogout().Bind(OnLogout);
        }

        void OnLoginReq(EventMasterLoginReq req)
        {
            // Skip DB processing for now

            Entry user;

            if ( !dic.TryGetValue(req.Account, out user) )
            {
                user = new Entry();
            }
            else
            {
                // TODO: kickout existing user.  

                if ( user.Handle != req._Handle)
                {
                    
                }
            }

            user.Account = req.Account;
            user.Guid = 0;
            user.Handle = req._Handle;

            dic[user.Account] = user;

            new EventMasterLoginResp
            {
                Account = user.Account,
                Result = 0
            }
            .Post();
        }

        void OnLogout(EventMasterLogout req)
        {
            dic.Remove(req.Account);
        }
    }
}
