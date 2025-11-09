using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Order")]
public  class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }
    [ForeignKey("Customer")]
    [Required]
    public int? CustomerId { get; set; }
    [ForeignKey("Resource")]
    public int? ResourceId { get; set; }
    [ForeignKey("Service")]
    public int? ServiceId { get; set; }
    [ForeignKey("Staff")]
    public int? StaffId { get; set; }

    [Column(TypeName ="decimal(10,2)") ]
    [Precision(10, 2)]
    public decimal? TotalPrice { get; set; }
    [Required]
    [DefaultValue("Processing")]
    public string Status { get; set; }
    [DefaultValue("COD") ]
    public string? PaymentMethod { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Cân nặng đồ không được âm.")]
    public double? Weight { get; set; }
    [DataType(DataType.DateTime, ErrorMessage = "Ngày không đúng định dạng.")]
    public DateTime? CreateAt { get; set; }
    [DataType(DataType.DateTime)]
    public DateTime? PickupAt { get; set; }   // Hẹn lấy

    [DataType(DataType.DateTime, ErrorMessage = "Ngày không đúng định dạng.")]

    public DateTime? UpdateAt { get; set; }

    public virtual Customer? Customer { get; set; } = null!;

    public virtual ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();

    public virtual Resource? Resource { get; set; }

    public virtual Service? Service { get; set; } = null!;

    public virtual Staff? Staff { get; set; }
}
