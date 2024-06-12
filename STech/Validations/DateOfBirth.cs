using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace STech.Validations
{
    public class DateOfBirth : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime dob = Convert.ToDateTime(value);
            if(dob != null && (dob < Convert.ToDateTime("1930/01/01") || dob >= DateTime.Now))
            {
                return new ValidationResult(this.ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}