using LaundryWPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaundryWPF.Helpers;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace LaundryWPF.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly Sem7Prn212Context _context;
        // danh sách cho các order trễ
        public ObservableCollection<Order> LateOrders { get; set; } = new();

        // bộ đếm lateOrder 
        private int _lateCount;
        public int LateCount
        {
            get => _lateCount;
            set
            {
                _lateCount = value;
                OnPropertyChanged();
            }
        }

        public HomePageViewModel()
        {
            LoadLateOrder();
        }

        public void LoadLateOrder()
        {
            try
            {
                using var context = new Sem7Prn212Context();
                var lateOrders = context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Staff)
                    .Include(o => o.Service)
                    .Where(o =>
                    o.Status != "Complete" &&
                    o.PickupAt != null &&
                    o.PickupAt < DateTime.Now

                    ).OrderBy(o => o.PickupAt).ToList();

                LateOrders.Clear();
                foreach (var order in lateOrders)
                {
                    LateOrders.Add(order);
                }
                LateCount = LateOrders.Count();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading late orders: {ex.Message}");
            }


        } 
    }
}
