// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebServiceRepository.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace THI_Analysis.Utility
{
    public abstract class WebServiceRepository
    {
        #region Public Methods and Operators

        public HttpWebRequest GetBaseHttpWebRequest(string endpointFragment, string authToken = null)
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(new Uri(BaseUri, endpointFragment));

            httpWebRequest.Timeout = RequestTimeOut;

            if (CustomHeaders.Any())
                foreach (var header in CustomHeaders)
                    httpWebRequest.Headers.Add(header.Key, header.Value);

            if (!string.IsNullOrEmpty(authToken))
            {
                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.CookieContainer.Add(BaseUri, new Cookie("sessionId", authToken));
            }

            return httpWebRequest;
        }

        #endregion

        #region Properties

        protected abstract string AuthenticationCookieName { get; }

        protected abstract Uri BaseUri { get; }


        protected abstract Dictionary<string, string> CustomHeaders { get; }

        protected abstract int RequestTimeOut { get; }

        #endregion

        #region Methods

        protected string IssueAuthenticatedHttpGet(string endpointFragment, string authToken)
        {
            var httpWebRequest = GetBaseHttpWebRequest(endpointFragment, authToken);


            return GetHttpWebRequestResponse(httpWebRequest);
        }

        protected string IssueAuthenticatedHttpJsonPost<TPostData>(
            string endpointFragment,
            TPostData postData,
            string authToken)
        {
            var httpWebRequest = GetBaseHttpWebRequest(endpointFragment, authToken);

            var jsonPostData = JsonConvert.SerializeObject(postData);
            var bytePostData = Encoding.UTF8.GetBytes(jsonPostData);


            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = bytePostData.Length;
            httpWebRequest.ContentType = "application/json";

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytePostData, 0, bytePostData.Length);
            }

            return GetHttpWebRequestResponse(httpWebRequest);
        }

        protected string IssueHttpGet(string endpointFragment)
        {
            return IssueAuthenticatedHttpGet(endpointFragment, null);
        }

        protected string IssueHttpJsonPost<TPostData>(string endpointFragment, TPostData postData, string authToken)
        {
            return IssueAuthenticatedHttpJsonPost(endpointFragment, postData, authToken);
        }

        protected TResult ParseHttpResponse<TResult>(string jsonResponse)
        {
            TResult parsedResult;
            using (var reader = new JsonTextReader(new StringReader(jsonResponse)))
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings());
                parsedResult = serializer.Deserialize<TResult>(reader);
                reader.Close();
            }
            return parsedResult;
        }

        private string GetHttpWebRequestResponse(HttpWebRequest request)
        {
            var response = string.Empty;

            using (var httpWebResponse = request.GetResponse())
            {
                if (request.HaveResponse)
                    using (var receiveStream = httpWebResponse.GetResponseStream())
                    {
                        if (receiveStream != null)
                            using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
                            {
                                response = readStream.ReadToEnd();
                            }
                    }
            }

            return response;
        }

        #endregion
    }
}