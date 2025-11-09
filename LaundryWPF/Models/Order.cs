using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

public partial class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }
    [ForeignKey("Customers")]
    public int? CustomerId { get; set; }
    [ForeignKey("Resource")]
    public int? ResourceId { get; set; }
    [ForeignKey("Services")]
    public int? ServiceId { get; set; }

    [Column(TypeName ="decimal(10,2)")]
    public decimal? TotalPrice { get; set; }
    [DeniedValues("active")]
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
