using Microsoft.AspNet.Identity.EntityFramework;

namespace QService.Admin
{
    public class User : IdentityUser
    {
        public bool Active { get; set; }
        public User()
        {
        }
    }
}
