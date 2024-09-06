using dong_backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dong_backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets representing the tables in the database
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserExpense> UserExpenses { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Calling the base method to include default Identity configuration
            base.OnModelCreating(modelBuilder);

            // Configuring the Log entity
            modelBuilder.Entity<Log>()
                .Property(l => l.Level)
                .IsRequired()                     // Making the Level field required
                .HasMaxLength(50);                // Setting max length of Level field

            modelBuilder.Entity<Log>()
                .Property(l => l.RequestMethod)
                .HasMaxLength(10);                // Setting max length of RequestMethod field

            // Configuring relationships for Expense entity
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.NoAction); // Preventing cascade delete for Group relation

            modelBuilder.Entity<Group>()
                .HasOne(g => g.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(g => g.Owner)
                .OnDelete(DeleteBehavior.Cascade);  // Enabling cascade delete for Group's Owner relation

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);  // Enabling cascade delete for Expense's CreatedBy relation

            // Configuring composite key for UserExpense entity
            modelBuilder.Entity<UserExpense>()
                .HasKey(ue => new { ue.UserId, ue.ExpenseId }); // Defining composite primary key

            modelBuilder.Entity<UserExpense>()
                .HasOne(ue => ue.User)
                .WithMany(u => u.UserExpenses)
                .HasForeignKey(ue => ue.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Preventing cascade delete for User relation in UserExpense

            modelBuilder.Entity<UserExpense>()
                .HasOne(ue => ue.Expense)
                .WithMany(e => e.UserExpenses)
                .HasForeignKey(ue => ue.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);  // Enabling cascade delete for Expense relation in UserExpense

            // Configuring composite key for UserGroup entity
            modelBuilder.Entity<UserGroup>()
                .HasKey(ug => new { ug.UserId, ug.GroupId }); // Defining composite primary key

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Preventing cascade delete for User relation in UserGroup

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.Group)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(ug => ug.GroupId)
                .OnDelete(DeleteBehavior.Cascade);  // Enabling cascade delete for Group relation in UserGroup

            // Setting default value for Group.Id using SQL Server's NEWID() function
            modelBuilder.Entity<Group>()
                .Property(g => g.Id)
                .HasDefaultValueSql("NEWID()");

            // Setting maximum length for Category property in Expense entity
            modelBuilder.Entity<Expense>()
                .Property(e => e.Category)
                .HasMaxLength(50);
        }
    }

}
