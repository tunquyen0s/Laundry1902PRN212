using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Customer")]
public  class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Tên khách hàng không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
    [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ.")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự.")]
    public string? Status { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số lần sử dụng dịch vụ phải là số không âm.")]
    public int? UseTime { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Ngày không đúng định dạng.")]
    public DateTime? CreateAt { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
