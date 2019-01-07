using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.CustomExceptions
{
    public class InvalidActionException : CustomRecruiterException
    {
        public InvalidActionException()
        {
        }
        public InvalidActionException(string message) : base(message)
        {
        }
        public InvalidActionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
