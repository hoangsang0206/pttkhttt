using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace STech.Helpers
{
    public static class UrlHelperExtension
    {
        public static string AddOrUpdateQueryParam(string baseUrl, string paramName, string paramValue)
        {
            var uriBuilder = new UriBuilder(baseUrl);
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);

            queryParams.Set(paramName, paramValue);
            uriBuilder.Query = queryParams.ToString();

            return uriBuilder.ToString();
        }

        public static string AddOrUpdateQueryParams(string baseUrl, NameValueCollection updatedParams)
        {
            var uriBuilder = new UriBuilder(baseUrl);
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (string key in updatedParams)
            {
                queryParams.Set(key, updatedParams[key]);
            }

            uriBuilder.Query = queryParams.ToString();

            return uriBuilder.ToString();
        }
    }
}