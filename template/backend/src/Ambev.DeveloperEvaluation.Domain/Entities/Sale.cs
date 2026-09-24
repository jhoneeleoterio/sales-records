using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale: BaseEntity
{
    public int Number { get; private set; }
    
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = String.Empty;

    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = String.Empty;
    
    public int Quantity { get; private set; }
    public decimal TotalAmount { get; private set; }
    
    public decimal Discount { get; private set; }
    public SaleStatus Status { get; private set; }
    
    private readonly List<SaleItem> _items = [];
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public static Sale Create(
        Guid customerId, 
        string customerName, 
        Guid branchId,
        string branchName, 
        List<SaleItem> items)
    {
        
        if (customerId.Equals(Guid.Empty)) throw new DomainException("Customer ID is required."); 
        if ((string.IsNullOrWhiteSpace(customerName))) throw new DomainException("Customer Name is required.");
        
        if (branchId.Equals(Guid.Empty)) throw new DomainException("Branch ID is required.");
        if (string.IsNullOrWhiteSpace(branchName)) throw new DomainException("Branch Name is required.");
        
        if (items is null || items.Count < 1) throw new DomainException("At least one item is required.");
        
        Sale sale = new()
        {
            CustomerId = customerId,
            CustomerName = customerName,
            BranchId = branchId,
            BranchName = branchName,
            Quantity = items.Select(s => s.Quantity).Sum(),
            Discount = items.Select(s => s.Discount).Sum(),
            TotalAmount = items.Sum(x => x.TotalAmount),
            Status = SaleStatus.NotCancelled
        };
        
        // Add SaleItems
        sale._items.AddRange(items);
                
        return sale;
    }

    public void Cancel()
    {
        Status = SaleStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    } 
}

public class SaleItem
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }

    public static SaleItem Create(
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        if (productId == Guid.Empty)  throw new DomainException("Product ID is required.");
        if (string.IsNullOrWhiteSpace(productName))  throw new DomainException("Product Name is required.");
        
        ValidatePrice(unitPrice);
        ValidateQuantity(quantity);
        
        var discount = ApplyDiscount(quantity, unitPrice);
        var totalAmount = CalculateTotalAmount(quantity, unitPrice, discount);
        
        SaleItem item = new()
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Discount = discount,
            TotalAmount = totalAmount
        };
        
        return item;
    }

    private static void ValidatePrice(decimal unitPrice)
    {
        if (unitPrice <= 0) 
            throw new DomainException("Price need greather than zero");
    }
    
    private static void ValidateQuantity(int quantity)
    {
        if (quantity is < 1 or > 20)
            throw new DomainException(
                "Quantity must be between 1 and 20."
            );
    }
    
    private static decimal ApplyDiscount(
        int quantity, 
        decimal unitPrice)
    {
        var discountRate = quantity switch
        {
            >= 10 and <= 20 => 0.20m,
            >= 4 and < 10 => 0.10m,
            _ => 0m
        };

        return quantity * unitPrice * discountRate;
    }

    private static decimal CalculateTotalAmount(
        int quantity,
        decimal unitPrice,
        decimal discount)
    {
        return quantity * unitPrice - discount;
    }

}