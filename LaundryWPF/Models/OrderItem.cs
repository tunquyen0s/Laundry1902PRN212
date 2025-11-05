using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("OrderItems")]
public partial class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderItemId { get; set; }

    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;  // Tên dịch vụ / sản phẩm trong đơn hàng

    public int? Quantity { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public virtual Order Order { get; set; } = null!;
}