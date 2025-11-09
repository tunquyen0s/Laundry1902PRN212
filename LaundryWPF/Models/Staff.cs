using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Staffs")] // Tên bảng trong database
public class Staff
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? StaffId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(50)]
    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salary { get; set; }

    public int? DayOff { get; set; }

    // Navigation property
    public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();
}