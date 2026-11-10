using Microsoft.EntityFrameworkCore;
using poe11.Models;

namespace poe11.RaceDayDatabase
{
    public class RaceDayDatabaseContext : DbContext
    {
        public RaceDayDatabaseContext(DbContextOptions<RaceDayDatabaseContext> options) : base(options) { }

        public DbSet<Users> Users => Set<Users>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventTypes> EventTypes => Set<EventTypes>();
        public DbSet<RouteMap> RouteMaps => Set<RouteMap>();
        public DbSet<RaceEntrance> RaceEntrances => Set<RaceEntrance>();
        public DbSet<Models.Results> Results => Set<Models.Results>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // EventTypeCode is the primary key; EF Core cannot infer it by convention.
            modelBuilder.Entity<EventTypes>().HasKey(t => t.EventTypeCode);
            modelBuilder.Entity<RaceEntrance>().HasKey(t => t.EntranceId);
            modelBuilder.Entity<Users>().HasKey(r => r.UserId);
            modelBuilder.Entity<Models.Results>().HasKey(r => r.ResultId);
            modelBuilder.Entity<Models.RouteMap>().HasKey(u => u.RouteId);
            modelBuilder.Entity<Event>().HasKey(u => u.EventId);

            modelBuilder.Entity<Users>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<RaceEntrance>().HasIndex(r => r.ChipNumber).IsUnique();
            modelBuilder.Entity<Models.Results>().HasIndex(r => r.EntranceId).IsUnique();
            modelBuilder.Entity<RouteMap>().HasIndex(r => r.EventTypeCode).IsUnique();

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer).WithMany()
                .HasForeignKey(e => e.OrganizerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EventTypes>()
                .HasOne(t => t.Event).WithMany(e => e.EventTypes)
                .HasForeignKey(t => t.EventId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RaceEntrance>()
                .HasOne(r => r.User).WithMany()
                .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RaceEntrance>()
                .HasOne(r => r.EventType).WithMany()
                .HasForeignKey(r => r.EventTypeCode).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Models.Results>()
                .HasOne(r => r.RaceEntrance).WithMany()
                .HasForeignKey(r => r.EntranceId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RouteMap>()
                .HasOne(r => r.EventType).WithMany()
                .HasForeignKey(r => r.EventTypeCode).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
