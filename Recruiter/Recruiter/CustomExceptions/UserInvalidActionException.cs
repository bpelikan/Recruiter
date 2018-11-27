using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.CustomExceptions
{
    public class UserInvalidActionException : Exception
    {
        public UserInvalidActionException()
        {
        }
        public UserInvalidActionException(string message) : base(message)
        {
        }
        public UserInvalidActionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
