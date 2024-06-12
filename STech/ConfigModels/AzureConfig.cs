using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;

namespace STech.ConfigModels
{
    public class AzureConfig
    {
        public static string GetConnectionString()
        {
            NameValueCollection azureSection = (NameValueCollection)ConfigurationManager.GetSection("azure");
            return azureSection["StripeApiKey"];
        }
    }
}