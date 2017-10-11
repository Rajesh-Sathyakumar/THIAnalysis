using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace THI_Analysis.Dto
{
    public class SearchUsersPostData
    {
        public string Keyword { get; set; }

        public int AccountStatus { get; set; }

        public int UserType { get; set; }

        public string SortColumn { get; set; }

        public int SortDirection { get; set; }

        public int Index { get; set; }

        public int NoofItems { get; set; }

        public Guid[] MemberOrgKeys { get; set; }

        public Guid ApplicationKey { get; set; }

        public Guid[] EnvironmentKeys { get; set; }

    }
}