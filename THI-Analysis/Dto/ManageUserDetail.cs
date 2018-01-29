// --------------------------------------------------------------------------------------------------------------------
// <copyright company="The Advisory Board Company">
// Copyright © 2014 The Advisory Board Company
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THI_Analysis.Dto
{
    using System;

    public class ManageUserDetail
    {
        #region Public Properties

        public int? UserKey { get; set; }

        public string UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string IsAdmin { get; set; }

        public Guid UserGuid { get; set; }

        public int UserRole { get; set; }

        public int UserProduct { get; set; }

        public string IsActive { get; set; }

        public List<string> Acls { get; set; }

        #endregion
    }
}