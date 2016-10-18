using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Action
{
    /// <summary>
    /// The return type when invoking action tree
    /// </summary>
    public enum ActionStatus
    {
        Success,
        Failure,
        Running
    }
}
