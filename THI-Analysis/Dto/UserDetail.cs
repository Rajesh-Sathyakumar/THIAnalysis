// --------------------------------------------------------------------------------------------------------------------
// <copyright company="The Advisory Board Company">
// Copyright © 2014 The Advisory Board Company
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace THI_Analysis.Dto
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    using THI_Analysis.Enums;

    public class UserDetail
    {
        #region Constructors and Destructors

        public UserDetail()
        {
            this.Members = new List<MemberDto>();
        }

        #endregion

        #region Public Properties

        public string CellPhone { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Department { get; set; }

        public DateTime? Dob { get; set; }

        public string Email { get; set; }

        public string EmployeeId { get; set; }

        public string Facility { get; set; }

        public string Fax { get; set; }

        public string FirstName { get; set; }

        public bool? IsDeactivated { get; set; }

        public bool? IsFullyProvisioned { get; set; }

        public string LastName { get; set; }

        public DateTime? LockoutDate { get; set; }

        public int? LockoutReason { get; set; }

        public Guid? MemberOrgKey { get; set; }

        public ICollection<MemberDto> Members { get; set; }

        public string OfficePhone { get; set; }

        public UserRole Role { get; set; }

        [JsonProperty("Speciality")]
        public string Specialty { get; set; }

        public string State { get; set; }

        public string Street { get; set; }

        public Guid UserKey { get; set; }

        public string UserName { get; set; }

        public string ValidationToken { get; set; }

        public int? Zip { get; set; }

        #endregion

        public class ApplicationDto
        {
            #region Constructors and Destructors

            public ApplicationDto()
            {
                this.Environments = new List<EnvironmentDto>();
            }

            #endregion

            #region Public Properties

            public Guid ApplicationId { get; set; }

            public IList<EnvironmentDto> Environments { get; set; }

            public string LongName { get; set; }

            public string ShortName { get; set; }

            #endregion
        }

        public class EnvironmentDto
        {
            #region Public Properties

            public Guid EnvironmentId { get; set; }

            public bool IsProvisioned { get; set; }

            public Guid MemberOrgApplicationEnvironemntKey { get; set; }

            public string Name { get; set; }

            public string WebApplicationUrl { get; set; }

            #endregion
        }

        public class MemberDto
        {
            #region Constructors and Destructors

            public MemberDto()
            {
                this.Applications = new List<ApplicationDto>();
            }

            #endregion

            #region Public Properties

            public IList<ApplicationDto> Applications { get; set; }

            public string LongName { get; set; }

            public Guid MemberOrgId { get; set; }

            public string ShortName { get; set; }

            #endregion
        }
    }
}