using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity.EntityFramework;
using STech.Identity;

[assembly: OwinStartup(typeof(STech.App_Start.OwinStartup))]

namespace STech.App_Start
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/")
            });

            this.CreateRolesAndUsers();
        }

        public void CreateRolesAndUsers()
        {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext()));

            if(!roleManager.RoleExists("Admin"))
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Customer"))
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Customer";
                roleManager.Create(role);
            }


            if (userManager.FindByName("admin") == null)
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = "admin";
                user.Email = "2001210561@huit.edu.vn";
                user.Avatar = "https://lhswebstorage.blob.core.windows.net/stechweb/user-images/admin-avatarpng";
                string password = "admin@2001210561";

                if(userManager.Create(user, password).Succeeded)
                {
                    userManager.AddToRole(user.Id, "Admin");
                };
            }
        }
    }
}
