using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Shared
{
    public static class RoleCollection
    {
        public const string Administrator = "Administrator";

        public const string Recruiter = "Recruiter";

        public const string Recruit = "Recruit";

        public const string Test = "Test";

        public static HashSet<string> Roles = new HashSet<string>() {
                    Administrator,
                    Recruiter,
                    Recruit,
                    Test
                };
    }
}
