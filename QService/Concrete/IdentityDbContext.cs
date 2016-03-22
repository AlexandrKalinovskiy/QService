using Microsoft.AspNet.Identity.EntityFramework;
using QService.Admin;

namespace QService.Concrete
{
    public partial class IdentityContext : IdentityDbContext<User>
    {
        public IdentityContext() : base("IdentityDbContext") { }

        public static IdentityDbContext Create()
        {
            return new IdentityDbContext();
        }
    }
}
