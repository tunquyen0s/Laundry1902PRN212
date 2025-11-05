using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaundryWPF.Models
{
    public class AppDbContext : DbContext
    {
  
        //Hàm dựng
        public AppDbContext() { }

        //Tạo các collection ánh xạ đến các bảng trong CSDL
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OderItems { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Staff> Staffs { get; set; }



        //Tạo phương thức lấy chuỗi kết nối từ file appsettings.json
        private string GetConnectionString()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var connectionstring = configuration["ConnectionStrings:DefaultConnection"];
            return connectionstring;
        }


        //Override lại phương thức OnConfiguring để thiết lập kết nối cho CSDL
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(GetConnectionString());
        }

        //Override lại phương thức OnModelCreating nếu cần cấu hình mô hình dữ liệu.

    }

}
