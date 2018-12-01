using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.CustomExceptions
{
    public class InvalidFileExtensionException : CustomException
    {
        public InvalidFileExtensionException()
        {
        }
        public InvalidFileExtensionException(string message) : base(message)
        {
        }
        public InvalidFileExtensionException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
