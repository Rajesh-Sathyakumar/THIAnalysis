// --------------------------------------------------------------------------------------------------------------------
// <copyright company="The Advisory Board Company">
// Copyright © 2014 The Advisory Board Company
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace THI_Analysis.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using THI_Analysis.Utility;

    public class ConfirmUserEnrollment
    {
        #region Constructors and Destructors

        public ConfirmUserEnrollment()
        {
            this.UserMemberOrgKeyList = new List<UserMemberOrgKeyPair>();
        }

        public ConfirmUserEnrollment(List<UserMemberOrgKeyPair> userMemberOrgKeyList, Guid appGuid, Guid envGuid)
        {
            this.UserMemberOrgKeyList = userMemberOrgKeyList;
            this.ApplicationKey = appGuid;
            this.EnvironmentKey = envGuid;
        }

        #endregion

        #region Public Properties

        public Guid ApplicationKey { get; set; }

        public Guid EnvironmentKey { get; set; }

        public List<UserMemberOrgKeyPair> UserMemberOrgKeyList { get; set; }

        #endregion

        
       }
}