using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration: IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    { 
        builder.ToTable("Sales");   
        builder.HasKey(sale => sale.Id);
        
        builder.Property(sale => sale.Id)
               .HasColumnType("uuid")
               .HasDefaultValueSql("gen_random_uuid()")
               .ValueGeneratedOnAdd();
        
        builder.Property(sale => sale.Number)
               .UseIdentityAlwaysColumn();
        
        builder.Property(sale => sale.CustomerId)
               .HasColumnType("uuid")
               .IsRequired();
        
        builder.Property(sale => sale.CustomerName)
               .HasMaxLength(100)
               .IsRequired();
        
        builder.Property(sale => sale.BranchId)
               .HasColumnType("uuid")
               .IsRequired();
        
        builder.Property(sale => sale.BranchName)
               .HasMaxLength(100)
               .IsRequired();
        
        builder.Property(sale => sale.Quantity)
               .IsRequired();
        
        builder.Property(sale => sale.TotalAmount)
               .HasPrecision(18, 2)
               .IsRequired();
        
        builder.Property(sale => sale.Discount)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(sale => sale.Status)
               .HasConversion<string>()
               .HasMaxLength(20);
       
        builder.Navigation(sale => sale.Items)
               .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.OwnsMany(
               sale => sale.Items, item =>
              {
                     item.ToTable("SaleItems");
                     item.WithOwner().HasForeignKey("SaleId");
                     item.Property(p=> p.ProductId).HasColumnType("uuid").IsRequired();
                     item.Property(p => p.ProductName).HasMaxLength(100).IsRequired();
                     item.Property(p => p.UnitPrice).HasPrecision(18, 2).IsRequired();
                     item.Property(p => p.Quantity).IsRequired();
                     item.Property(p => p.TotalAmount).HasPrecision(18, 2).IsRequired();
                     item.Property(p => p.Discount).HasPrecision(18, 2).IsRequired();
              });
    }
}