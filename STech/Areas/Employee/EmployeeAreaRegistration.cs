using System.Web.Mvc;

namespace STech.Areas.Employee
{
    public class EmployeeAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Employee";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Employee_orders",
                "employee/",
                new { controller = "Orders", action = "Index" }
            );

            context.MapRoute(
                "Employee_default",
                "employee/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}