using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THI_Analysis.Dto
{
    public class EnrollUsersResultDto
    {
        public EnrollUsersResultDto()
        {
            this.UserMemberOrgKeyList = new List<UserMemberOrgKeyPair>();
        }

        public IEnumerable<UserMemberOrgKeyPair> UserMemberOrgKeyList { get; set; }

       
        public class UserMemberOrgKeyPair
        {
            public Guid UserKey { get; set; }

            public Guid MemberOrgKey { get; set; }

            public bool Status { get; set; }
        }
    }
}
