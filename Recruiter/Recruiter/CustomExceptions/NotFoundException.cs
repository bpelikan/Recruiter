using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.CustomExceptions
{
    public class NotFoundException : CustomException
    {
        public NotFoundException()
        {
        }
        public NotFoundException(string message) : base(message)
        {
        }
        public NotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
