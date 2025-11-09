using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Resource")]
public class Resource
{
    public int ResourceId { get; set; }

    [Required(ErrorMessage = "Loại không được để trống!")]
    public string Type { get; set; } = null!;
    [Required(ErrorMessage = "Tên không được để trống!")]
    public string Name { get; set; } = null!;
    
    public string? Unit { get; set; }
   
    public string? Description { get; set; }
    [Required(ErrorMessage = "Giá không thể để trống và phải là số.")]
    [Range (0, 1000, ErrorMessage = "Giá không thể bé hơn 0.")]
    public decimal? PricePerUnit { get; set; }
    [Required(ErrorMessage = "Số lượng không được để trống và phải là số.")]
    [Range(0,1000, ErrorMessage ="Số lượng không thể bé hơn 0.")]
    public decimal? Quantity { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

}
