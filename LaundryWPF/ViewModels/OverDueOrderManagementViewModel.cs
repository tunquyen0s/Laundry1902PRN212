using LaundryWPF.Helpers;
using LaundryWPF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LaundryWPF.ViewModels
{
    public class OverDueOrderManagementViewModel : BaseViewModel
    {
        private readonly Sem7Prn212Context _context = new Sem7Prn212Context();
        private readonly EmailService _emailService = new EmailService();

        public ObservableCollection<Order> LateOrders { get; set; } = new();

        private int _lateCount;
        public int LateCount
        {
            get => _lateCount;
            set { _lateCount = value; OnPropertyChanged(); }
        }

        private Order? _selectedOrder;
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Cập nhật trạng thái nút
            }
        }

        public ICommand SendMailCommand { get; }

        public OverDueOrderManagementViewModel()
        {
            LoadLateOrder();
            SendMailCommand = new RelayCommand(async (_) => await SendMailAsync(), (_) => SelectedOrder != null);
        }

        public void LoadLateOrder()
        {
            try
            {
                var lateOrders = _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Staff)
                    .Include(o => o.Service)
                    .Where(o => o.Status != "Complete"
                             && o.PickupAt != null
                             && o.PickupAt < DateTime.Now)
                    .OrderBy(o => o.PickupAt)
                    .ToList();

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

        private async Task SendMailAsync()
        {
            // Kiểm tra email khách hàng
            if (SelectedOrder?.Customer?.Email == null)
            {
                MessageBox.Show("Khách hàng không có email!");
                return;
            }

            // Lấy dữ liệu cần thiết
            string customerName = SelectedOrder.Customer.Name;
            int orderId = SelectedOrder.OrderId;
            // Sử dụng ngày hẹn lấy đồ (PickupAt) làm ngày hoàn thành dự kiến
            string completionDate = SelectedOrder.PickupAt?.ToString("dd/MM/yyyy") ?? "Không rõ";

            string to = SelectedOrder.Customer.Email;
            string subject = $"🚨 Thông báo đơn hàng {orderId} đã quá thời hạn lấy đồ tại Laundry1902";

            // Xây dựng nội dung email theo mẫu
            string body = $"Kính gửi Khách hàng **{customerName}**, <br/><br/>" +
                          $"Chúng tôi gửi email này để nhắc nhở bạn về đơn hàng giặt sấy **{orderId}** của bạn đã hoàn thành và **quá thời hạn lấy đồ** tại cửa hàng theo quy định.<br/><br/>" +
                          $"---<br/>" +
                          $"**Mã đơn hàng:** {orderId}<br/>" +
                          $"**Ngày hoàn thành dự kiến:** {completionDate}<br/>" +
                          $"---<br/><br/>" +
                          $"Để đảm bảo chất lượng đồ giặt và tránh tình trạng lưu kho quá lâu, chúng tôi kính mong bạn sắp xếp thời gian đến **Laundry1902** để nhận lại đồ của mình trong thời gian sớm nhất.<br/><br/>" +
                          $"Xin chân thành cảm ơn sự hợp tác của bạn!<br/><br/>" +
                          $"Trân trọng,<br/>" +
                          $"**Đội ngũ Laundry1902**";

            // Gửi mail
            await _emailService.SendEmailAsync(to, subject, body);
        }
    }
}
