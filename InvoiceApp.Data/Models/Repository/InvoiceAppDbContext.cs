using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApp.Data.Models.Repository
{
    public class InvoiceAppDbContext : IdentityDbContext<ApplicationUser>
    {
        public InvoiceAppDbContext(DbContextOptions<InvoiceAppDbContext> options) : base(options) 
        {

        }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Invoices) 
                .WithOne(i => i.User) 
                .HasForeignKey(i => i.UserID) 
                .IsRequired(); 

            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.InvoiceID); 

            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.Items) 
                .WithOne(it => it.Invoice) 
                .HasForeignKey(it => it.InvoiceID); 

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.SenderAddress) 
                .WithMany() 
                .HasForeignKey(i => i.SenderAddressID) 
                .IsRequired() 
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.ClientAddress) 
                .WithMany() 
                .HasForeignKey(i => i.ClientAddressID) 
                .IsRequired() 
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Item>()
                .HasKey(it => it.ItemID); 

            modelBuilder.Entity<Address>()
                .HasKey(a => a.AddressID); 

        }
    }
}
