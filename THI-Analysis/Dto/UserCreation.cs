// --------------------------------------------------------------------------------------------------------------------
// <copyright company="The Advisory Board Company">
// Copyright © 2014 The Advisory Board Company
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace THI_Analysis.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using THI_Analysis.Models;
    using THI_Analysis.Utility;

    public class UserCreation
    {
        #region Constructors and Destructors

        public UserCreation()
        {
            this.UserAccounts = new List<UsersToBeCreatedDto>();
        }

        public UserCreation(List<UsersToBeCreatedDto> userData, List<Moa> memberApplicationEnvironmentKeys)
        {
            this.UserAccounts = userData;
            this.MemberApplicationEnvironmentKeys = memberApplicationEnvironmentKeys;
        }

        public UserCreation(DataTable userData, Guid applicationGuid, Guid memberGuid, Guid environmentGuid)
        {
            this.UserAccounts =
                userData.AsEnumerable()
                    .Select(
                        row =>
                        new UsersToBeCreatedDto
                            {
                                FirstName = Convert.ToString(row.Field<string>("FirstName")), 
                                LastName = Convert.ToString(row.Field<string>("LastName")), 
                                Email = Convert.ToString(row.Field<string>("Email")), 
                                UserName = Convert.ToString(row.Field<string>("UserName")), 
                                EmployeeId = Convert.ToString(row.Field<string>("EmployeeId")), 
                                Cellphone = Convert.ToString(row.Field<string>("Cellphone")), 
                                Dob = Convert.ToString(row.Field<string>("Dob")), 
                                MemberApplicationEnvironmentIndex = new List<int> { 0 }
                            })
                    .ToList();

            this.MemberApplicationEnvironmentKeys = new List<Moa>
                                                        {
                                                            new Moa
                                                                {
                                                                    MemberOrgKey = memberGuid, 
                                                                    ApplicationKey = applicationGuid, 
                                                                    EnvironmentKey = environmentGuid
                                                                }
                                                        };
        }

     

        #endregion

        #region Public Properties

        public List<Moa> MemberApplicationEnvironmentKeys { get; set; }

        public List<UsersToBeCreatedDto> UserAccounts { get; set; }

        #endregion

        public class Moa
        {
            #region Public Properties

            public Guid ApplicationKey { get; set; }

            public Guid EnvironmentKey { get; set; }

            public Guid MemberOrgKey { get; set; }

            #endregion
        }

        public class UsersToBeCreatedDto
        {
            #region Public Properties

            public string Cellphone { get; set; }

            public string Dob { get; set; }

            public string Email { get; set; }

            public string EmployeeId { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public IList<int> MemberApplicationEnvironmentIndex { get; set; }

            public string UserName { get; set; }

            #endregion
        }
    }
}