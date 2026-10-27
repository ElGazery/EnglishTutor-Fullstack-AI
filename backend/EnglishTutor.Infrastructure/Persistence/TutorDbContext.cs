using EnglishTutor.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Infrastructure.Persistence
{
    public class TutorDbContext : IdentityDbContext<ApplicationUser>
    {
        public TutorDbContext(DbContextOptions<TutorDbContext> options) : base(options)
        {
        }

        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all IEntityTypeConfiguration<T> from this assembly
            builder.ApplyConfigurationsFromAssembly(typeof(TutorDbContext).Assembly);
        }
    }
}
