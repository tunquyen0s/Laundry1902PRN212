using System;
using System.Collections.Generic;

namespace LaundryWPF.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public decimal? Salary { get; set; }

    public int? DayOff { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
