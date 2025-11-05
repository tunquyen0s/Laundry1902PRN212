using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Customers")]
public partial class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(50)]
    public string? Status { get; set; }

    public int? UseTime { get; set; }

    public DateTime? CreateAt { get; set; } = DateTime.Now;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
