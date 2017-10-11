// --------------------------------------------------------------------------------------------------------------------
// <copyright company="The Advisory Board Company">
// Copyright © 2014 The Advisory Board Company
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace THI_Analysis.Dto
{
    using System;

    public class UserStatus
    {
        #region Public Properties

        public Guid MemberOrgKey { get; set; }

        public string Status { get; set; }

        public Guid Userkey { get; set; }

        #endregion
    }
}
