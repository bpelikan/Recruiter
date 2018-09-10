using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface ICvStorage
    {
        Task<string> SaveCv(Stream CvStream);
        string UriFor(string cvId);
    }
}
