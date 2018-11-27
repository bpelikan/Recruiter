using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.CustomExceptions
{
    public class PermissionException : CustomException
    {
        public PermissionException()
        {
        }
        public PermissionException(string message) : base(message)
        {
        }
        public PermissionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}