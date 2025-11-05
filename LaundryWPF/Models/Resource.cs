using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Resources")]
public partial class Resource
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResourceId { get; set; }

    [Required]
    [StringLength(100)]
    public string Type { get; set; } = null!;  // Loại tài nguyên (ví dụ: Nguyên liệu, Thiết bị...)

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;  // Tên tài nguyên

    [StringLength(50)]
    public string? Unit { get; set; }          // Đơn vị đo (kg, cái, lít...)

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PricePerUnit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Quantity { get; set; }

    // Quan hệ 1-n: 1 Resource có thể được dùng trong nhiều Orders
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}