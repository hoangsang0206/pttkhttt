using System.Web;
using System.Web.Optimization;

namespace STech
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/css").Include(
                      "~/Css/style.css",
                      "~/Css/home.css",
                      "~/Css/collections.css",
                      "~/Css/product.css",
                      "~/Css/account.css",
                      "~/Css/cart.css"));

            bundles.Add(new ScriptBundle("~/scripts").Include(
                "~/Scripts/script.js",
                "~/Scripts/products.js",
                "~/Scripts/cart.js",
                "~/Scripts/account.js",
                "~/Scripts/order.js"));
        }
    }
}
