using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    /* The DataContext class is a DbContext that contains a DbSet of AppUser objects. */
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// The OnModelCreating function is used to configure the relationships and constraints between
        /// entities in the database model.
        /// </summary>
        /// <param name="ModelBuilder">ModelBuilder is a class provided by Entity Framework Core that is
        /// used to configure the database model. It is used to define the shape of the entities and
        /// their relationships in the database.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserLike>()
            .HasKey(k=> new{k.SourceUserId, k.TargetUserId});

            modelBuilder.Entity<UserLike>()
            .HasOne(s=> s.SourceUser)
            .WithMany(l=> l.LikedUsers)
            .HasForeignKey(s=> s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<UserLike>()
            .HasOne(s=> s.TargetUser)
            .WithMany(l=> l.LikedByUsers)
            .HasForeignKey(s=> s.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
            .HasOne(u=> u.Recipient)
            .WithMany(m=> m.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Message>()
            .HasOne(u=> u.Sender)
            .WithMany(m=> m.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}