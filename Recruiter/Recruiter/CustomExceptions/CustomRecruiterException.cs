using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.CustomExceptions
{
    public class CustomRecruiterException : Exception
    {
        public CustomRecruiterException()
        {
        }
        public CustomRecruiterException(string message) : base(message)
        {
        }
        public CustomRecruiterException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
