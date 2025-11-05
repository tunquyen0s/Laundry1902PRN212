using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Services")]
public partial class Service
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ServiceId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PricePerUnit { get; set; }

    // Thời gian thực hiện (phút/giờ tuỳ ý)
    public int? TimeCost { get; set; }

    // Quan hệ 1-n: 1 dịch vụ có thể thuộc nhiều đơn hàng
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
