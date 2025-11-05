using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Orders")]
public partial class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }

    // 🔹 Foreign Keys
    [ForeignKey(nameof(Customer))]
    public int CustomerId { get; set; }

    [ForeignKey(nameof(Resource))]
    public int? ResourceId { get; set; }

    [ForeignKey(nameof(Service))]
    public int ServiceId { get; set; }

    [ForeignKey(nameof(Staff))]
    public int? StaffId { get; set; }

    // 🔹 Data fields
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalPrice { get; set; }

    [StringLength(50)]
    public string? Status { get; set; } // Ví dụ: "Pending", "Completed", "Cancelled"

    [StringLength(50)]
    public string? PaymentMethod { get; set; } // Ví dụ: "Cash", "Credit Card", "Bank Transfer"

    public double? Weight { get; set; } // Trọng lượng đơn hàng

    public DateTime? CreateAt { get; set; } = DateTime.Now;

    public DateTime? UpdateAt { get; set; }

    // 🔹 Navigation properties
    public virtual Customer Customer { get; set; } = null!;

    public virtual Resource? Resource { get; set; }

    public virtual Service Service { get; set; } = null!;

    public virtual Staff? Staff { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
