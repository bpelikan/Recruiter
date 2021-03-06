﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface ICvStorageService
    {
        Task<string> SaveCvAsync(Stream CvStream, string fileName, string userId);
        Task<bool> DeleteCvAsync(string cvId);  //delete return bool
        string UriFor(string cvId);
    }
}
