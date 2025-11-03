using System;
using System.Collections.Generic;

namespace LaundryWPF.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public int? ResourceId { get; set; }

    public int? ServiceId { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public string? PaymentMethod { get; set; }

    public double? Weight { get; set; }

    public int? StaffId { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual Customer? Customer { get; set; } = null!;

    public virtual ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();

    public virtual Resource? Resource { get; set; }

    public virtual Service? Service { get; set; } = null!;

    public virtual Staff? Staff { get; set; } = null!;
}
