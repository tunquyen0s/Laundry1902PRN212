using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [MaxLength(100)]
    public string? Name { get; set; } = null!;
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public int? DayOff { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
