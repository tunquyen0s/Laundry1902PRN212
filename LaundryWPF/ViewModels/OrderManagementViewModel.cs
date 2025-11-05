using LaundryWPF.Helpers;
using LaundryWPF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LaundryWPF.ViewModels
{
    public class OrderManagementViewModel : BaseViewModel
    {
        // =================== DỮ LIỆU NGUỒN ===================
        private List<Order> _allOrders;
        public ObservableCollection<Order> Orders { get; set; }
        public ObservableCollection<Customer> AllCustomers { get; set; }
        public ObservableCollection<Service> AllServices { get; set; }
        public ObservableCollection<Staff> AllStaff { get; set; }
        public ObservableCollection<Resource> AllResources { get; set; }
        public ObservableCollection<OrderItem> NewOrderItems { get; set; }

        // =================== FILTER ===================
        private bool _showCompletedOrders;
        public bool ShowCompletedOrders
        {
            get => _showCompletedOrders;
            set { _showCompletedOrders = value; OnPropertyChanged(); ApplyFilter(); }
        }

        // =================== ĐƠN HÀNG ĐANG CHỌN ===================
        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                if (value != null) MapOrderToForm();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private OrderItem _selectedOrderItem;
        public OrderItem SelectedOrderItem
        {
            get => _selectedOrderItem;
            set
            {
                _selectedOrderItem = value;
                OnPropertyChanged();
                if (value != null) MapOrderItemToForm();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // =================== FORM ORDER ===================
        private Customer _selectedCustomer;
        public Customer SelectedCustomer { get => _selectedCustomer; set { _selectedCustomer = value; OnPropertyChanged(); } }

        private Service _selectedService;
        public Service SelectedService { get => _selectedService; set { _selectedService = value; OnPropertyChanged(); } }

        private Staff _selectedStaff;
        public Staff SelectedStaff { get => _selectedStaff; set { _selectedStaff = value; OnPropertyChanged(); } }

        private Resource _selectedResource;
        public Resource SelectedResource { get => _selectedResource; set { _selectedResource = value; OnPropertyChanged(); } }

        private double? _weight;
        public double? Weight { get => _weight; set { _weight = value; OnPropertyChanged(); } }

        // =================== FORM ITEM ===================
        private string _newItemName;
        public string NewItemName { get => _newItemName; set { _newItemName = value; OnPropertyChanged(); } }

        private int? _newItemQuantity;
        public int? NewItemQuantity { get => _newItemQuantity; set { _newItemQuantity = value; OnPropertyChanged(); } }

        private string _newItemDescription;
        public string NewItemDescription { get => _newItemDescription; set { _newItemDescription = value; OnPropertyChanged(); } }

        // =================== COMMANDS ===================
        public ICommand CreateOrderCommand { get; }
        public ICommand SaveOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand UpdateItemCommand { get; }
        public ICommand DeleteItemCommand { get; }

        public ICommand CompleteOrderCommand { get; }

        // =================== CONSTRUCTOR ===================
        public OrderManagementViewModel()
        {
            _allOrders = new List<Order>();
            Orders = new ObservableCollection<Order>();
            AllCustomers = new ObservableCollection<Customer>();
            AllServices = new ObservableCollection<Service>();
            AllStaff = new ObservableCollection<Staff>();
            AllResources = new ObservableCollection<Resource>();
            NewOrderItems = new ObservableCollection<OrderItem>();

            CreateOrderCommand = new RelayCommand(obj => ExecuteCreateOrder(obj));
            SaveOrderCommand = new RelayCommand(obj => ExecuteSaveOrder(obj));
            DeleteOrderCommand = new RelayCommand(obj => ExecuteDeleteOrder(obj), obj => SelectedOrder != null);

            AddItemCommand = new RelayCommand(obj => ExecuteAddItem());
            UpdateItemCommand = new RelayCommand(obj => ExecuteUpdateItem(), obj => SelectedOrderItem != null);
            DeleteItemCommand = new RelayCommand(obj => ExecuteDeleteItem(), obj => SelectedOrderItem != null);

            CompleteOrderCommand = new RelayCommand(obj => ExecuteCompleteOrder(), obj => SelectedOrder != null && SelectedOrder.OrderId > 0);

            LoadData();
            CommandManager.InvalidateRequerySuggested();
        }

        // =================== LOAD DATA ===================
        private void LoadData()
        {
            using var context = new AppDbContext();
            AllCustomers = new ObservableCollection<Customer>(context.Customers.ToList());
            AllServices = new ObservableCollection<Service>(context.Services.ToList());
            AllStaff = new ObservableCollection<Staff>(context.Staffs.ToList());
            AllResources = new ObservableCollection<Resource>(context.Resources.ToList());

            _allOrders = context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Service)
                .Include(o => o.Staff)
                .Include(o => o.Resource)
                .Include(o => o.OrderItems)
                .ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Orders.Clear();
            var filtered = ShowCompletedOrders ? _allOrders : _allOrders.Where(o => o.Status != "Completed");
            foreach (var o in filtered)
                Orders.Add(o);
        }

        // =================== MAPPING ===================
        private void MapOrderToForm()
        {
            if (SelectedOrder == null) { ClearForm(); return; }

            SelectedCustomer = AllCustomers.FirstOrDefault(c => c.CustomerId == SelectedOrder.CustomerId);
            SelectedService = AllServices.FirstOrDefault(s => s.ServiceId == SelectedOrder.ServiceId);
            SelectedStaff = AllStaff.FirstOrDefault(s => s.StaffId == SelectedOrder.StaffId);
            SelectedResource = AllResources.FirstOrDefault(r => r.ResourceId == SelectedOrder.ResourceId);
            Weight = SelectedOrder.Weight;

            NewOrderItems.Clear();
            foreach (var item in SelectedOrder.OrderItems)
                NewOrderItems.Add(new OrderItem
                {
                    OrderItemId = item.OrderItemId,
                    OrderId = item.OrderId,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Description = item.Description
                });
        }

        private void MapOrderItemToForm()
        {
            if (SelectedOrderItem == null) return;
            NewItemName = SelectedOrderItem.Name;
            NewItemQuantity = SelectedOrderItem.Quantity;
            NewItemDescription = SelectedOrderItem.Description;
        }

        private void ClearForm()
        {
            SelectedOrder = null;
            SelectedCustomer = null;
            SelectedService = null;
            SelectedStaff = null;
            SelectedResource = null;
            Weight = null;
            NewOrderItems.Clear();
            ClearOrderItemForm();
        }

        private void ClearOrderItemForm()
        {
            NewItemName = string.Empty;
            NewItemQuantity = null;
            NewItemDescription = string.Empty;
            SelectedOrderItem = null;
        }

        // =================== CRUD ORDER ===================
        private void ExecuteCreateOrder(object obj)
        {
            // Tạo đơn hàng tạm, chưa lưu DB
            var newOrder = new Order
            {
                Status = "Processing",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            // Gán làm SelectedOrder, hiển thị trên giao diện
            SelectedOrder = newOrder;
            NewOrderItems.Clear();

            MessageBox.Show("Đã tạo đơn hàng mới (chưa lưu vào cơ sở dữ liệu)!");
        }

        private void ExecuteSaveOrder(object obj)
        {
            if (SelectedCustomer == null || SelectedService == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng và dịch vụ!");
                return;
            }

            using var context = new AppDbContext();

            // Nếu là đơn mới (chưa có trong DB)
            if (SelectedOrder.OrderId == 0)
            {
                SelectedOrder.CustomerId = SelectedCustomer.CustomerId;
                SelectedOrder.ServiceId = SelectedService.ServiceId;
                SelectedOrder.StaffId = SelectedStaff?.StaffId;
                SelectedOrder.ResourceId = SelectedResource?.ResourceId;
                SelectedOrder.Weight = Weight;
                SelectedOrder.UpdateAt = DateTime.Now;
                SelectedOrder.TotalPrice = SelectedService.PricePerUnit * (decimal)(Weight ?? 1.0);

                context.Orders.Add(SelectedOrder);
                context.SaveChanges();

                MessageBox.Show("Đã lưu đơn hàng mới vào cơ sở dữ liệu!");
            }
            else
            {
                // Đơn hàng đã tồn tại → cập nhật
                var order = context.Orders.Include(o => o.OrderItems)
                                          .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);
                if (order == null) return;

                order.CustomerId = SelectedCustomer.CustomerId;
                order.ServiceId = SelectedService.ServiceId;
                order.StaffId = SelectedStaff?.StaffId;
                order.ResourceId = SelectedResource?.ResourceId;
                order.Weight = Weight;
                order.UpdateAt = DateTime.Now;
                order.TotalPrice = SelectedService.PricePerUnit * (decimal)(Weight ?? 1.0);

                context.SaveChanges();
                MessageBox.Show("Đã cập nhật đơn hàng!");
            }

            LoadData();
            SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);
        }





        private void ExecuteDeleteOrder(object obj)
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Vui lòng chọn đơn hàng để xóa!");
                return;
            }

            if (MessageBox.Show("Xác nhận xóa đơn hàng này?", "Xóa đơn hàng", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.No) return;

            using var context = new AppDbContext();
            var order = context.Orders.Include(o => o.OrderItems)
                                      .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

            if (order != null)
            {
                context.OderItems.RemoveRange(order.OrderItems);
                context.Orders.Remove(order);
                context.SaveChanges();

                LoadData();
                SelectedOrder = null;
                MessageBox.Show("Đã xóa đơn hàng!");
            }
        }


        // =================== CRUD ITEM ===================
      private void ExecuteAddItem()
{
    if (SelectedOrder == null)
    {
        MessageBox.Show("Vui lòng tạo hoặc chọn đơn hàng trước khi thêm món!");
        return;
    }

    var newItem = new OrderItem
    {
        Name = NewItemName ?? "",
        Quantity = NewItemQuantity,
        Description = NewItemDescription ?? ""
    };

    // Nếu đơn hàng chưa lưu DB → chỉ thêm vào danh sách tạm
    if (SelectedOrder.OrderId == 0)
    {
        NewOrderItems.Add(newItem);
        SelectedOrder.OrderItems.Add(newItem);
        MessageBox.Show("Đã thêm món (chưa lưu vào cơ sở dữ liệu)!");
    }
    else
    {
        // Đơn hàng đã lưu DB → thêm thật vào DB
        using var context = new AppDbContext();
        var order = context.Orders.Include(o => o.OrderItems)
                                  .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);
        if (order == null)
        {
            MessageBox.Show("Không tìm thấy đơn hàng trong cơ sở dữ liệu!");
            return;
        }

        order.OrderItems.Add(newItem);
        context.SaveChanges();

        LoadData();
        SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == order.OrderId);
        MessageBox.Show("Đã thêm món vào đơn hàng!");
    }

    ClearOrderItemForm();
}

        private void ExecuteUpdateItem()
        {
            if (SelectedOrderItem == null)
            {
                MessageBox.Show("Vui lòng chọn món để cập nhật!");
                return;
            }

            using var context = new AppDbContext();
            var item = context.OderItems.FirstOrDefault(i => i.OrderItemId == SelectedOrderItem.OrderItemId);
            if (item == null)
            {
                MessageBox.Show("Không tìm thấy món trong cơ sở dữ liệu!");
                return;
            }

            item.Name = NewItemName ?? "";
            item.Quantity = NewItemQuantity;
            item.Description = NewItemDescription ?? "";
            context.SaveChanges();

            // Lưu lại OrderId trước khi LoadData (tránh SelectedOrder bị null)
            int? currentOrderId = SelectedOrder?.OrderId;

            LoadData();

            // Chỉ tìm lại SelectedOrder nếu còn OrderId hợp lệ
            if (currentOrderId.HasValue)
                SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == currentOrderId.Value);

            ClearOrderItemForm();
            MessageBox.Show("Đã cập nhật món!");
        }
        private void ExecuteDeleteItem()
        {
            if (SelectedOrderItem == null)
            {
                MessageBox.Show("Vui lòng chọn món để xóa!");
                return;
            }

            if (MessageBox.Show("Xác nhận xóa món này?", "Xóa món", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.No) return;

            using var context = new AppDbContext();
            var item = context.OderItems.FirstOrDefault(i => i.OrderItemId == SelectedOrderItem.OrderItemId);
            if (item != null)
            {
                context.OderItems.Remove(item);
                context.SaveChanges();
            }
            // Lưu lại OrderId trước khi LoadData (tránh SelectedOrder bị null)
            int? currentOrderId = SelectedOrder?.OrderId;
            LoadData();
            // Chỉ tìm lại SelectedOrder nếu còn OrderId hợp lệ
            if (currentOrderId.HasValue)
                SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == currentOrderId.Value);
            ClearOrderItemForm();
            MessageBox.Show("Đã xóa món!");
        }


        private void ExecuteCompleteOrder()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Vui lòng chọn đơn hàng cần hoàn thành!");
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn đánh dấu đơn hàng này là HOÀN THÀNH?",
                                "Xác nhận hoàn thành",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            using var context = new AppDbContext();
            var order = context.Orders.FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);
            if (order == null)
            {
                MessageBox.Show("Không tìm thấy đơn hàng trong cơ sở dữ liệu!");
                return;
            }

            order.Status = "Completed";
            order.UpdateAt = DateTime.Now;
            context.SaveChanges();

            LoadData();
            MessageBox.Show("Đơn hàng đã được đánh dấu là HOÀN THÀNH!");
        }

    }
}
