using System.Web;
using System.Web.Optimization;

namespace STech
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/css").Include(
                      "~/CSS/style.css",
                      "~/CSS/home.css",
                      "~/CSS/collections.css",
                      "~/CSS/product.css",
                      "~/CSS/account.css",
                      "~/CSS/cart.css"));

            bundles.Add(new ScriptBundle("~/scripts").Include(
                "~/Scripts/script.js",
                "~/Scripts/products.js",
                "~/Scripts/cart.js",
                "~/Scripts/account.js",
                "~/Scripts/order.js"));
        }
    }
}
