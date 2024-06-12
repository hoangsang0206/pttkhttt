using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;

namespace STech.ConfigModels
{
    public class StripeConfig
    {
        public static string GetApiKey()
        {
            NameValueCollection stripeSection = (NameValueCollection)ConfigurationManager.GetSection("stripe");
            return stripeSection["StripeApiKey"];
        }
    }
}