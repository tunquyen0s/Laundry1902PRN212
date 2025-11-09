using LaundryWPF.Helpers;
using LaundryWPF.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LaundryWPF.ViewModels
{
    public class OrderManagementViewModel : BaseViewModel
    {
        // =================== DỮ LIỆU NGUỒN ===================
        private List<Order> _allOrders = new();
        public ObservableCollection<Order> Orders { get; set; } = new();
        public ObservableCollection<Customer> AllCustomers { get; set; } = new();
        public ObservableCollection<Service> AllServices { get; set; } = new();
        public ObservableCollection<Staff> AllStaff { get; set; } = new();
        public ObservableCollection<Resource> AllResources { get; set; } = new();
        public ObservableCollection<OrderItem> NewOrderItems { get; set; } = new();

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
                if (value != null)
                    MapOrderItemToForm();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // =================== FORM ORDER ===================
        public Customer SelectedCustomer { get => _selectedCustomer; set { _selectedCustomer = value; OnPropertyChanged(); } }
        private Customer _selectedCustomer;

        public Service SelectedService { get => _selectedService; set { _selectedService = value; OnPropertyChanged(); } }
        private Service _selectedService;

        public Staff SelectedStaff { get => _selectedStaff; set { _selectedStaff = value; OnPropertyChanged(); } }
        private Staff _selectedStaff;

        public Resource SelectedResource { get => _selectedResource; set { _selectedResource = value; OnPropertyChanged(); } }
        private Resource _selectedResource;

        public double? Weight { get => _weight; set { _weight = value; OnPropertyChanged(); } }
        private double? _weight;

        // =================== FORM ITEM ===================
        public string NewItemName { get => _newItemName; set { _newItemName = value; OnPropertyChanged(); } }
        private string _newItemName;

        public int? NewItemQuantity { get => _newItemQuantity; set { _newItemQuantity = value; OnPropertyChanged(); } }
        private int? _newItemQuantity;

        public string NewItemDescription { get => _newItemDescription; set { _newItemDescription = value; OnPropertyChanged(); } }
        private string _newItemDescription;

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
            CreateOrderCommand = new RelayCommand(_ => ExecuteCreateOrder());
            SaveOrderCommand = new RelayCommand(_ => ExecuteSaveOrder());
            DeleteOrderCommand = new RelayCommand(_ => ExecuteDeleteOrder(), _ => SelectedOrder != null);

            AddItemCommand = new RelayCommand(_ => ExecuteAddItem());
            UpdateItemCommand = new RelayCommand(_ => ExecuteUpdateItem(), _ => SelectedOrderItem != null);
            DeleteItemCommand = new RelayCommand(_ => ExecuteDeleteItem(), _ => SelectedOrderItem != null);

            CompleteOrderCommand = new RelayCommand(_ => ExecuteCompleteOrder(), _ => SelectedOrder != null && SelectedOrder.OrderId > 0);

            LoadData();
        }

        // =================== LOAD DATA ===================
        private void LoadData()
        {
            using var context = new Prn212Context();
            AllCustomers = new ObservableCollection<Customer>(context.Customers.ToList());
            AllServices = new ObservableCollection<Service>(context.Services.ToList());
            AllStaff = new ObservableCollection<Staff>(context.Staff.ToList());
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
            var filtered = ShowCompletedOrders
                ? _allOrders
                : _allOrders.Where(o => o.Status != "Completed");
            foreach (var o in filtered)
                Orders.Add(o);
        }

        // =================== MAPPING ===================
        private void MapOrderToForm()
        {
            if (SelectedOrder == null)
            {
                ClearForm();
                return;
            }

            SelectedCustomer = AllCustomers.FirstOrDefault(c => c.CustomerId == SelectedOrder.CustomerId);
            SelectedService = AllServices.FirstOrDefault(s => s.ServiceId == SelectedOrder.ServiceId);
            SelectedStaff = AllStaff.FirstOrDefault(s => s.StaffId == SelectedOrder.StaffId);
            SelectedResource = AllResources.FirstOrDefault(r => r.ResourceId == SelectedOrder.ResourceId);
            Weight = SelectedOrder.Weight;

            NewOrderItems.Clear();
            if (SelectedOrder.OrderItems != null)
            {
                foreach (var item in SelectedOrder.OrderItems)
                {
                    NewOrderItems.Add(new OrderItem
                    {
                        OrderItemId = item.OrderItemId,
                        OrderId = item.OrderId,
                        Name = item.Name,
                        Quantity = item.Quantity,
                        Description = item.Description
                    });
                }
            }
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
        private void ExecuteCreateOrder()
        {
            var newOrder = new Order
            {
                Status = "Processing",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };

            Orders.Insert(0, newOrder);
            SelectedOrder = newOrder;
            NewOrderItems.Clear();

            MessageBox.Show("Đã tạo đơn hàng mới (chưa lưu vào cơ sở dữ liệu)!");
        }

        private void ExecuteSaveOrder()
        {
            using var context = new Sem7Prn212Context();

            if (SelectedCustomer == null || SelectedService == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng và dịch vụ trước khi lưu!");
                return;
            }

            using var context = new Prn212Context();
            int savedOrderId = SelectedOrder?.OrderId ?? 0;

            if (savedOrderId == 0)
            {
                // === Lưu đơn hàng mới ===
                SelectedOrder.CustomerId = SelectedCustomer.CustomerId;
                SelectedOrder.ServiceId = SelectedService.ServiceId;
                SelectedOrder.StaffId = SelectedStaff?.StaffId;
                SelectedOrder.ResourceId = SelectedResource?.ResourceId;
                SelectedOrder.Weight = Weight;
                SelectedOrder.TotalPrice = SelectedService.PricePerUnit * (decimal)(Weight ?? 1.0);
                SelectedOrder.UpdateAt = DateTime.Now;

                context.Orders.Add(SelectedOrder);
                context.SaveChanges();

                savedOrderId = SelectedOrder.OrderId;
                MessageBox.Show("Đã lưu đơn hàng mới vào cơ sở dữ liệu!");
            }
            else
            {
                // === Cập nhật đơn hàng có sẵn ===
                var order = context.Orders.Include(o => o.OrderItems)
                                          .FirstOrDefault(o => o.OrderId == savedOrderId);
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
            SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == savedOrderId);
        }

        private void ExecuteDeleteOrder()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Vui lòng chọn đơn hàng để xóa!");
                return;
            }

            if (MessageBox.Show("Xác nhận xóa đơn hàng này?", "Xóa đơn hàng", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            using var context = new Prn212Context();
            var order = context.Orders.Include(o => o.OrderItems)
                                      .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

            if (order != null)
            {
                context.OrderItems.RemoveRange(order.OrderItems);
                context.Orders.Remove(order);
                context.SaveChanges();
                MessageBox.Show("Đã xóa đơn hàng!");
            }

            LoadData();
            SelectedOrder = null;
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
        using var context = new Prn212Context();
        var order = context.Orders.Include(o => o.OrderItems)
                                  .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);
        if (order == null)
        {
            MessageBox.Show("Không tìm thấy đơn hàng trong cơ sở dữ liệu!");
            return;
        }

                order.OrderItems.Add(newItem);
                context.SaveChanges();
                MessageBox.Show("Đã thêm món vào đơn hàng!");

                LoadData();
                SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == order.OrderId);
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

            int? currentOrderId = SelectedOrder?.OrderId;

            using var context = new Sem7Prn212Context();
            var item = context.OrderItems.FirstOrDefault(i => i.OrderItemId == SelectedOrderItem.OrderItemId);
            if (item == null)
            {
                MessageBox.Show("Không tìm thấy món trong cơ sở dữ liệu!");
                return;
            }

            item.Name = NewItemName ?? "";
            item.Quantity = NewItemQuantity;
            item.Description = NewItemDescription ?? "";
            context.SaveChanges();

            LoadData();
            if (currentOrderId.HasValue)
                SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == currentOrderId.Value);

            MessageBox.Show("Đã cập nhật món!");
        }

        private void ExecuteDeleteItem()
        {
            if (SelectedOrderItem == null)
            {
                MessageBox.Show("Vui lòng chọn món để xóa!");
                return;
            }

            if (MessageBox.Show("Xác nhận xóa món này?", "Xóa món", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            int? currentOrderId = SelectedOrder?.OrderId;

            using var context = new Prn212Context();
            var item = context.OrderItems.FirstOrDefault(i => i.OrderItemId == SelectedOrderItem.OrderItemId);
            if (item != null)
            {
                context.OrderItems.Remove(item);
                context.SaveChanges();
            }

            LoadData();
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
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            using var context = new Prn212Context();
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
            SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == order.OrderId);
            MessageBox.Show("Đơn hàng đã được đánh dấu là HOÀN THÀNH!");
        }
    }
}
