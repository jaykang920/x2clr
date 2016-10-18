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
        LoginBase= 2001,
        LoginReq,
        LoginResp,
        Logout,
        MasterLoginReq,
        MasterLoginResp,
        MasterLogout,
        End
    }
}
