using System;
using System.Collections.Generic;

namespace LaundryWPF.Models;

public partial class Resource
{
    public int ResourceId { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Unit { get; set; }

    public string? Description { get; set; }

    public decimal? PricePerUnit { get; set; }

    public decimal? Quantity { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
