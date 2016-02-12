using QService.Entities;
using System.Data.Entity;

namespace QService.Concrete
{
    public partial class EFDbContext : DbContext
    {
        public virtual DbSet<ExchangeBoard> ExchangeBoards { get; set; }
        public virtual DbSet<Security> Securities { get; set; }
    }
}
