using STech.Identity;
using STech.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http;

namespace STech.Controllers_api
{
    public class Authenticate : ApiController
    {
        [HttpPost]
        public IHttpActionResult Login(LoginVM login)
        {
            
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Register(RegisterVM register)
        {
            if(ModelState.IsValid)
            {
                ApplicationUserManager userManager = new ApplicationUserManager(new ApplicationUserStore(new ApplicationDbContext()));
                ApplicationUser user = new ApplicationUser()
                {
                    UserName = register.RegUsername,
                    Email = register.RegEmail,
                    PasswordHash = Crypto.HashPassword(register.RegPassword)
                };

                if(userManager.FindByNameAsync(register.RegUsername) != null) { }
            }

            return Ok();
        }
    }
}