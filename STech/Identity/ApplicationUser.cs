using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STech.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public String UserFullName { get; set; }
        public String Gender { get; set; }
        public DateTime? DOB {  get; set; }
        public String Address {  get; set; }
        public String Avatar { get; set; }

    }
}