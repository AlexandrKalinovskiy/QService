using Microsoft.AspNet.Identity.EntityFramework;
using QService.Admin;
using QService.Entities;
using System.Data.Entity;

namespace QService.Concrete
{
    public partial class IdentityContext : IdentityDbContext<User>
    {
        public IdentityContext() : base("IdentityDbContext") { }

        public static IdentityDbContext Create()
        {
            return new IdentityDbContext();
        }

        new public DbSet<ApplicationRole> Roles { get; set; }
    }
}
