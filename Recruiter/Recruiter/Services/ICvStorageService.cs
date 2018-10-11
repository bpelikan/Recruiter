using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface ICvStorageService
    {
        Task<string> SaveCvAsync(Stream CvStream, string userId, string ext);
        Task<bool> DeleteCvAsync(string cvId);
        string UriFor(string cvId);
    }
}
