using Microsoft.EntityFrameworkCore;
using LiveStock.Core.Models;

namespace LiveStock.Infrastructure.Data
{
    public class LiveStockDbContext : DbContext
    {
        private const string V = "";

        public LiveStockDbContext(DbContextOptions<LiveStockDbContext> options) : base(options)
        {
        }

        public DbSet<Sheep> Sheep { get; set; }
        public DbSet<Cow> Cows { get; set; }
        public DbSet<Camp> Camps { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<CampMovement> CampMovements { get; set; }
        public DbSet<RainfallRecord> RainfallRecords { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<FarmTask> FarmTasks { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<FinancialRecord> FinancialRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Sheep>()
                .HasOne(s => s.Camp)
                .WithMany(c => c.Sheep)
                .HasForeignKey(s => s.CampId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cow>()
                .HasOne(c => c.Camp)
                .WithMany(c => c.Cows)
                .HasForeignKey(c => c.CampId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix decimal precision warnings
            modelBuilder.Entity<MedicalRecord>()
                .Property(m => m.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CampMovement>()
                .HasOne(cm => cm.FromCamp)
                .WithMany()
                .HasForeignKey(cm => cm.FromCampId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CampMovement>()
                .HasOne(cm => cm.ToCamp)
                .WithMany()
                .HasForeignKey(cm => cm.ToCampId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmTask>()
                .HasOne(t => t.AssignedTo)
                .WithMany(s => s.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmTask>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(s => s.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany(s => s.ReceivedMessages)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asset>()
                .Property(a => a.PurchasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinancialRecord>()
                .Property(f => f.Amount)
                .HasPrecision(18, 2);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed 15 camps
            for (int i = 1; i <= 15; i++)
            {
                modelBuilder.Entity<Camp>().HasData(new Camp
                {
                    Id = i,
                    Name = $"Camp {i}",
                    CampNumber = i,
                    Hectares = 153.33, // 2300 hectares / 15 camps
                    Description = $"Camp {i} - Livestock grazing area"
                });
            }

            // Seed default staff (Farmer)
            modelBuilder.Entity<Staff>().HasData(new Staff
            {
                Id = 1,
                Name = "Farm Owner",
                EmployeeId = "FARM001",
                PhoneNumber = "+1234567890",
                Email = "owner@farm.com",
                Role = "Farmer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // Seed sample financial records
            modelBuilder.Entity<FinancialRecord>().HasData(
                new FinancialRecord
                {
                    Id = 1,
                    Type = "Income",
                    Description = "Sheep Sale - Merino Ewe",
                    Amount = 450.00m,
                    TransactionDate = DateTime.UtcNow.AddDays(-10),
                    Category = "Livestock Sales",
                    Reference = "INV-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new FinancialRecord
                {
                    Id = 2,
                    Type = "Expense",
                    Description = "Veterinary Services",
                    Amount = 150.00m,
                    TransactionDate = DateTime.UtcNow.AddDays(-15),
                    Category = "Veterinary",
                    Reference = "REC-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new FinancialRecord
                {
                    Id = 3,
                    Type = "Expense",
                    Description = "Feed Purchase",
                    Amount = 300.00m,
                    TransactionDate = DateTime.UtcNow.AddDays(-20),
                    Category = "Feed",
                    Reference = "INV-002",
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                }
            );

            // Seed sample assets
            modelBuilder.Entity<Asset>().HasData(
                new Asset
                {
                    Id = 1,
                    Name = "Fencing Wire",
                    Category = "Fencing",
                    Type = "Barbed Wire",
                    Description = "High-quality barbed wire for camp fencing",
                    Quantity = 50,
                    Unit = "rolls",
                    PurchasePrice = 25.00m,
                    PurchaseDate = DateTime.UtcNow.AddDays(-60),
                    Status = "Active",
                    Location = "Storage Shed A",
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                },
                new Asset
                {
                    Id = 2,
                    Name = "Livestock Feed",
                    Category = "Feed",
                    Type = "Grain Mix",
                    Description = "Premium grain mix for livestock",
                    Quantity = 1000,
                    Unit = "kg",
                    PurchasePrice = 0.80m,
                    PurchaseDate = DateTime.UtcNow.AddDays(-30),
                    ExpiryDate = DateTime.UtcNow.AddMonths(6),
                    Status = "Active",
                    Location = "Feed Storage",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                }
            );
        }
    }
} 