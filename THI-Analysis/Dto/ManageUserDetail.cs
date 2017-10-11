// --------------------------------------------------------------------------------------------------------------------
// <copyright company="The Advisory Board Company">
// Copyright © 2014 The Advisory Board Company
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THI_Analysis.Dto
{
    using System;

    public class ManageUserDetail
    {
        #region Public Properties

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public Guid UserGuid { get; set; }  

        #endregion
    }
}