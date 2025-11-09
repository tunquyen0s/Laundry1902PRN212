using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LaundryWPF.Models;

public partial class Sem7Prn212Context : DbContext
{
    public Sem7Prn212Context()
    {
    }

    public Sem7Prn212Context(DbContextOptions<Sem7Prn212Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    private string GetConnectionString()
    {
        string currentDir = Directory.GetCurrentDirectory();
        string appSettingsPath = Path.Combine(currentDir, "appsettings.json");
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(currentDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Không tìm thấy chuỗi kết nối 'DefaultConnection' bên trong file appsettings.json.");
        }

        return connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());
}
