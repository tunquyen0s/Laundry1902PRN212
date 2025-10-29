using System;
using System.Collections.Generic;

namespace LaundryWPF.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? PricePerUnit { get; set; }

    public int? TimeCost { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
