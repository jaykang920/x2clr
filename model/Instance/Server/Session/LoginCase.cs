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
        public LoginCase()
            : base()
        {
        }

        protected override void Setup()
        {
            base.Setup();

            new EventLoginReq().Bind(OnLoginReq);
            new EventMasterLoginResp().Bind(OnLoginResp);
        }

        void OnLoginReq(EventLoginReq req)
        {
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
