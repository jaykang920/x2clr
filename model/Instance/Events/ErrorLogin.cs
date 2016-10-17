using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    /// <summary>
    /// ErrorCodes for all protocol
    /// Use descriptive name to explain error
    /// </summary>
    enum ErrorLogin
    {
        Success = 0, 
        FailLogin = ErrorCodes.LoginBegin,
        FailLogin_UserNotExist,
        FailLogin_DuplicateLogin, 
        FailLogin_PasswordIncorrect,
    }
}
