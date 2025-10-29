using System;
using System.Collections.Generic;

namespace LaundryWPF.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public string Name { get; set; } = null!;

    public int? Quantity { get; set; }

    public string? Description { get; set; }

    public virtual Order Order { get; set; } = null!;
}
