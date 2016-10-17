using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    // range from 1001 ~ 2000
    public enum EventClusterTypes
    {
        ServerList = 1001, 
        Join,
        Leave,
        End,
    }
}
