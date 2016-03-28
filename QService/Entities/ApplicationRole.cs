using Microsoft.AspNet.Identity.EntityFramework;

namespace QService.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public int NumberOfThreads { get; set; }

        public ApplicationRole()
        {
        }
    }
}
