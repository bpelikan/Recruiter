using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.CustomExceptions
{
    public class BlobOperationException : CustomRecruiterException
    {
        public BlobOperationException()
        {
        }
        public BlobOperationException(string message) : base(message)
        {
        }
        public BlobOperationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
