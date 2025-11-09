using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

public partial class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }
    [Required(ErrorMessage ="Tên đồ không được trống")]
    public string? Name { get; set; } = null!;
    [Range(1, int.MaxValue,ErrorMessage ="Cần ít nhất 1 món đồ")]
    public int? Quantity { get; set; }

    public string? Description { get; set; }

    public virtual Order Order { get; set; } = null!;
}
