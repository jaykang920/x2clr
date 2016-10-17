using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    // range from 2001 ~ 3000
    public enum EventLoginTypes
    {
        LoginReq = 2001,
        LoginResp,
        Logout,
        MasterLoginReq,
        MasterLoginResp,
        MasterLogout,
        End
    }
}
