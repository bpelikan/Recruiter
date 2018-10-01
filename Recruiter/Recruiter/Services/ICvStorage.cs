using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface ICvStorage
    {
        Task<string> SaveCvAsync(Stream CvStream, string userId);
        Task<bool> DeleteCvAsync(string cvId);
        string UriFor(string cvId);
    }
}
