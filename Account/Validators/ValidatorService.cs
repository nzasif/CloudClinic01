using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CloudClinic.Account.Validators
{
    public static class ValidatorService
    {
        public static bool IsValidateEmail(string input)
        {
            //do email validation
            Regex regex = new System.Text.RegularExpressions.Regex("^[a-zA-z0-9]@[a-zA-Z].[a-zA-Z0-9]", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (regex.IsMatch(input))
            {
                return true;
            }
            else
                return false;
        }
    }
}
