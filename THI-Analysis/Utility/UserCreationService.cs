// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserCreationService.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace THI_Analysis.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;


    using THI_Analysis.Dto;

    public class UserCreationService : WebServiceRepository
    {
        #region Constants

        private const int ApiVersion = 2;

        #endregion

        #region Fields

        private string _provisioningServiceBaseUrl;

        #endregion

        #region Properties

        protected override string AuthenticationCookieName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override Uri BaseUri
        {
            get
            {
                return new Uri(this._provisioningServiceBaseUrl);
            }
        }

        protected override Dictionary<string, string> CustomHeaders
        {
            get
            {
                var customHeaders = new Dictionary<string, string> { { "Version", ApiVersion.ToString() } };
                return customHeaders;
            }
        }



        protected override int RequestTimeOut
        {
            get
            {
                return Convert.ToInt32(1000000);
            }
        }

        #endregion

        #region Public Methods and Operators

        public string ConfirmUsersEnrollment(
            ConfirmUserEnrollment parameters, 
            string apiBaseUrl, 
            string confirmUserEnrollmentsEndpoint, 
            string authToken)
        {
            this._provisioningServiceBaseUrl = apiBaseUrl;
            var response = this.IssueHttpJsonPost(confirmUserEnrollmentsEndpoint, parameters, authToken);

            
            return response;
        }

        public IEnumerable<UserDetail> GetUnprovisionedUsers(
            Guid productGuid, 
            Guid memberGuid, 
            Guid environmentGuid, 
            string apiBaseUrl, 
            string getUnprovisionedUsersEndpoint, 
            string authToken)
        {
            this._provisioningServiceBaseUrl = apiBaseUrl;
            var endpointFragment = string.Format(
                getUnprovisionedUsersEndpoint, 
                memberGuid, 
                productGuid, 
                environmentGuid);
            
            var response = this.IssueAuthenticatedHttpGet(endpointFragment, authToken);
            return this.ParseHttpResponse<IEnumerable<UserDetail>>(response);
        }

        public IEnumerable<UserDetail> GetDeactivatedUsers(SearchUsersPostData parameters, string apiBaseUrl, string searchUserEndPoint, string authToken)
        {
            this._provisioningServiceBaseUrl = apiBaseUrl;
            var response = this.IssueAuthenticatedHttpJsonPost<SearchUsersPostData>(searchUserEndPoint, parameters, authToken);
            var pagedResults = JObject.Parse(response)["PagedResults"].ToString();
            return this.ParseHttpResponse<IEnumerable<UserDetail>>(pagedResults);
        }

        public int GetDeactivatedUsersCount(UsersCount parameters, string apiBaseUrl, string getUsersCountEndPoint, string authToken)
        {
            var response = this.IssueAuthenticatedHttpJsonPost<UsersCount>(getUsersCountEndPoint, parameters, authToken);
            var jsonResponse = JObject.Parse(response)["totalUsersCount"][1]["TotalUsers"];
            return int.Parse(jsonResponse.ToString());
        }



        #endregion
    }
}