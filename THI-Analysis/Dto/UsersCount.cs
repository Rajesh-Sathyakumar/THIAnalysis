using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace THI_Analysis.Dto
{
    public class UsersCount
    {
        public Guid[] MemberOrgKeys { get; set; }
        public Guid ApplicationKey { get; set; }
        public Guid[] EnvironmentKeys { get; set; }
    }
}