using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    /// <summary>
    /// 4001 ~ 5000
    /// </summary>
    public enum EventInstanceTypes
    {
        Base = 4001, 
        MatchReq, 
        MatchResp, 
        CreateReq, 
        CreateResp, 
        JoinReq, 
        JoinResp, 
        LeaveReq, 
        LeaveResp,
        Status, 
        RunnerStatus
    }
}
