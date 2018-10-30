using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Shared
{
    public static class JobPositionActivity
    {
        public const string Planned = "Planned";

        public const string Active = "Active";

        public const string Expired = "Expired";

        public static HashSet<string> Activities = new HashSet<string>() {
                    Active,
                    Planned,
                    Expired,
        };
    }
}
