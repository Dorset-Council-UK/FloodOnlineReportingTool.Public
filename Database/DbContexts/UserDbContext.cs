using FloodOnlineReportingTool.Database.Models.Contact;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.Database.DbContexts;

public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext<FortUser>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaNames.FortUsers);

        base.OnModelCreating(modelBuilder);
    }
}
