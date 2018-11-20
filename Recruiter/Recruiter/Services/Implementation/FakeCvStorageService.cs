using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services.Implementation
{
    public class FakeCvStorageService : ICvStorageService
    {
        public Task<bool> DeleteCvAsync(string cvId)
        {
            return Task.FromResult(true);
        }

        public Task<string> SaveCvAsync(Stream CvStream, string userId, string ext)
        {
            return Task.FromResult("FakeCv");
        }

        public string UriFor(string cvId)
        {
            return "https://www.google.pl/";
        }
    }
}
