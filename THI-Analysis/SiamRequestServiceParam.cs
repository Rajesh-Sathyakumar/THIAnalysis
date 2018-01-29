using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace THI_Analysis
{
    public class SiamRequestServiceParam
    {
        #region Public Properties

        public Uri BaseAddress { get; set; }

        public string ContentType { get; set; }

        public IEnumerable<KeyValuePair<string, string>> FormParam { get; set; }

        public string FragmentUrl { get; set; }

        public int HttpTimeOut { get; set; }

        public string MethodType { get; set; }

        public string PostData { get; set; }

        public string SamlResponse { get; set; }

        public string SiamApiVersion { get; set; }

        #endregion
    }
}