using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using x2;
using Events.Login;

namespace Server.Session
{
    public class LoginCase : Core.ChannelCase
    {
        class Entry
        {
            public enum Status
            {
                LoginToMasterRequested, 
                Login 
            }

            public string Account;
            public int Guid;
            public int Handle;         // From SessionServer
            public Status Stat;
        }

        Dictionary<string, Entry> dic;

        public LoginCase()
            : base()
        {
            dic = new Dictionary<string, Entry>();
        }

        protected override void Setup()
        {
            base.Setup();

            new EventLoginReq().Bind(OnLoginReq);
            new EventMasterLoginResp().Bind(OnLoginResp);
        }

        void OnLoginReq(EventLoginReq req)
        {

            Entry user;

            if (dic.TryGetValue(req.Account, out user))
            {
                // TODO: Process duplicate login
            }
            else
            {
                user = new Entry();
            }

            // Remember
            user.Account = req.Account;
            user.Guid = 0;
            user.Handle = req._Handle;
            user.Stat = Entry.Status.LoginToMasterRequested;

            dic[user.Account] = user;

            Post(
                new EventMasterLoginReq
                {
                    Account = req.Account,
                    Password = req.Password
                }
            );
        }

        void OnLoginResp(EventMasterLoginResp resp)
        {
            Entry user; 

            if ( dic.TryGetValue(resp.Account, out user))
            {
                user.Stat = Entry.Status.Login;
            }
            else
            {
                // TODO: Somehow removed from dic
            }

            Post(
                new EventLoginResp
                {
                    Account = resp.Account, 
                    Result = resp.Result
                }
            );
        }
    }
}
