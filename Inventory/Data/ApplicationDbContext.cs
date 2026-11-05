using Microsoft.EntityFrameworkCore;
using Inventory.Models;

namespace Inventory.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();

        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();

        public DbSet<ServiceJob> ServiceJobs => Set<ServiceJob>();
        public DbSet<ServiceJobItem> ServiceJobItems => Set<ServiceJobItem>();

        public DbSet<StockTransaction> StockTransactions
            => Set<StockTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.PartNumber)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .Property(p => p.PurchaseRate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.SellingPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockTransaction>().ToTable(tb => tb.UseSqlOutputClause(false));

            modelBuilder.Entity<StockTransaction>()
                .Property(t => t.QuantityChange)
                .HasPrecision(18, 3);

            modelBuilder.Entity<StockTransaction>()
                .Property(t => t.UnitCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockTransaction>()
                .HasIndex(t => new
                {
                    t.ProductId,
                    t.TransactionDate
                });

            modelBuilder.Entity<StockTransaction>()
                .HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasIndex(s => s.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<ServiceJob>()
                .HasIndex(s => s.JobCardNumber)
                .IsUnique();

            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SaleItem>()
                .Property(i => i.Quantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<SaleItem>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceJob>()
                .Property(s => s.LabourCharges)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceJob>()
                .Property(s => s.PartsTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceJobItem>()
                .Property(i => i.Quantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ServiceJobItem>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SaleItem>()
                .HasOne(i => i.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceJobItem>()
                .HasOne(i => i.ServiceJob)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.ServiceJobId)
                .OnDelete(DeleteBehavior.Restrict);


        }



    }
}
