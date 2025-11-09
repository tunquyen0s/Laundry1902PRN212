using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaundryWPF.Models;

[Table("Service")]
public class Service
{
    [Key]
    public int ServiceId { get; set; }

    // ====== Tên dịch vụ ======
    [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên dịch vụ phải từ 3 đến 100 ký tự.")]
    [RegularExpression(@"^[A-Za-zÀ-ỹ0-9\s\-.,()]+$", ErrorMessage = "Tên dịch vụ chỉ được chứa chữ, số và ký tự hợp lệ.")]
    [Display(Name = "Tên dịch vụ")]
    public string Name { get; set; } = null!;

    // ====== Mô tả ======
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    // ====== Giá dịch vụ ======
    [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
    [Range(1000, 10000000, ErrorMessage = "Giá dịch vụ phải từ 1.000 đến 10.000.000 VNĐ.")]
    [Display(Name = "Giá dịch vụ (VNĐ)")]
    public decimal? PricePerUnit { get; set; }

    // ====== Thời gian thực hiện ======
    [Required(ErrorMessage = "Thời gian thực hiện là bắt buộc.")]
    [Range(5, 1440, ErrorMessage = "Thời gian thực hiện phải từ 5 đến 1.440 phút (tối đa 1 ngày).")]
    [Display(Name = "Thời gian thực hiện (phút)")]
    public int? TimeCost { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    // ====== Factory Method: tạo mới dịch vụ hợp lệ ======
    public static Service CreateService(string name, string description, decimal price, int timeCost)
    {
        return new Service
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            PricePerUnit = Math.Round(price, 2),
            TimeCost = timeCost
        };
    }

    // ====== Validation: tự động đọc DataAnnotation và trả về danh sách lỗi ======
    public List<string> GetValidationErrors()
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(this);
        Validator.TryValidateObject(this, context, results, true);

        return results.Select(r => $"• {r.ErrorMessage}").ToList();
    }

    // ====== Seed dữ liệu mẫu (chạy 1 lần nếu DB trống) ======
    public static void SeedSampleData(Sem7Prn212Context context)
    {
        if (context.Services.Any()) return; // Đã có dữ liệu thì bỏ qua

        var sampleServices = new List<Service>
            {
                CreateService("Giặt ủi quần áo", "Dịch vụ giặt ủi cơ bản cho quần áo hằng ngày", 15000, 60),
                CreateService("Giặt khô áo vest", "Dành cho các loại vest cao cấp cần bảo quản kỹ", 50000, 120),
                CreateService("Ủi quần tây", "Ủi quần tây công sở phẳng phiu, gọn gàng", 10000, 30),
                CreateService("Giặt nệm", "Giặt nệm tận nơi, sạch khuẩn và thơm lâu", 200000, 240),
                CreateService("Giặt rèm cửa", "Giặt rèm, màn cửa tại nhà, không phai màu", 80000, 180)
            };

        context.Services.AddRange(sampleServices);
        context.SaveChanges();
    }
}
