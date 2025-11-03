using System;
using System.Collections.Generic;

namespace LaundryWPF.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? Name { get; set; } = null!;

    public string? PhoneNumber { get; set; } = null!;

    public string? Status { get; set; }

    public int? UseTime { get; set; }

    public DateTime? CreateAt { get; set; }

    public int? DayOff { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
