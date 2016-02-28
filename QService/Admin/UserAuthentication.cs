using System;
using System.IdentityModel.Selectors;
using System.Security.Authentication;

namespace QService.Admin
{
    public class UserAuthentication : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            var ok = (userName == "Alonso") && (password == "Op2oyxq");

            if(!ok)
                throw new AuthenticationException("Incorrect username or password.");
        }
    }
}
