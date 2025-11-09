// (Nội dung file CustomerManagementViewModel.cs không thay đổi so với lần sửa trước)
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
    public class CustomerManagementViewModel : BaseViewModel
    {
        // =================== DỮ LIỆU NGUỒN ===================
        private List<Customer> _allCustomers = new List<Customer>();
        public ObservableCollection<Customer> Customers { get; set; } = new ObservableCollection<Customer>();

        // Đảm bảo có 2 trạng thái Active và Inactive
        public ObservableCollection<string> CustomerStatuses { get; set; } = new ObservableCollection<string> { "Active", "Inactive" };

        // =================== FILTER / SEARCH ===================
        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value ?? string.Empty;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        // =================== KHÁCH HÀNG ĐANG CHỌN (Binding Form) ===================
        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                UpdateFormTitle();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // =================== FORM KHÁCH HÀNG (Binding) ===================
        private string _formTitle = "Tạo Khách hàng mới";
        public string FormTitle { get => _formTitle; set { _formTitle = value; OnPropertyChanged(); } }


        // =================== COMMANDS ===================
        public ICommand CreateCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand NewCustomerFormCommand { get; }

        // =================== CONSTRUCTOR ===================
        public CustomerManagementViewModel()
        {
            // Commands
            NewCustomerFormCommand = new RelayCommand(obj => ExecuteNewCustomerForm());

            // Logic Create
            CreateCommand = new RelayCommand(obj => ExecuteSave(isUpdate: false),
                                             obj => CanExecuteSave() && SelectedCustomer != null && SelectedCustomer.CustomerId == 0);

            // Logic Update
            UpdateCommand = new RelayCommand(obj => ExecuteSave(isUpdate: true),
                                             obj => CanExecuteSave() && SelectedCustomer != null && SelectedCustomer.CustomerId > 0);

            // Logic Delete
            DeleteCommand = new RelayCommand(obj => ExecuteDeleteCustomer(),
                                             obj => SelectedCustomer != null && SelectedCustomer.CustomerId > 0);

            LoadData();
            ExecuteNewCustomerForm();
        }

        // =================== LOAD & FILTER DATA ===================
        private void LoadData()
        {
            using var context = new Sem7Prn212Context();
            _allCustomers = context.Customers
                                   // Thay đổi dòng này: Sắp xếp theo CustomerId tăng dần
                                   .OrderBy(c => c.CustomerId)
                                   // Dòng cũ là: .OrderByDescending(c => c.CreateAt)
                                   .ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Customers.Clear();
            var filteredList = _allCustomers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string searchLower = SearchTerm.ToLower();
                filteredList = filteredList.Where(c => c.Name.ToLower().Contains(searchLower) ||
                                                       c.PhoneNumber.Contains(searchLower) ||
                                                       (c.Email != null && c.Email.ToLower().Contains(searchLower)));
            }

            foreach (var customer in filteredList)
                Customers.Add(customer);
        }

        // =================== FORM MANAGEMENT & VALIDATION ===================
        private void UpdateFormTitle()
        {
            FormTitle = SelectedCustomer == null || SelectedCustomer.CustomerId == 0
                ? "Tạo Khách hàng mới"
                : $"Cập nhật Khách hàng: {SelectedCustomer.Name}";
        }

        private bool CanExecuteSave()
        {
            return SelectedCustomer != null &&
                   !string.IsNullOrWhiteSpace(SelectedCustomer.Name) &&
                   !string.IsNullOrWhiteSpace(SelectedCustomer.Email) &&
                   !string.IsNullOrWhiteSpace(SelectedCustomer.PhoneNumber);
        }

        private bool IsValidCustomer(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                MessageBox.Show("Tên khách hàng không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(customer.Email) || !System.Net.Mail.MailAddress.TryCreate(customer.Email, out _))
            {
                MessageBox.Show("Email không hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
            {
                MessageBox.Show("Số điện thoại không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public void ExecuteNewCustomerForm()
        {
            SelectedCustomer = new Customer
            {
                CustomerId = 0,
                Name = string.Empty,
                Email = string.Empty,
                PhoneNumber = string.Empty,

                CreateAt = DateTime.Now,
                Status = CustomerStatuses.FirstOrDefault() ?? "Active",
                UseTime = 0,
            };
            UpdateFormTitle();
            CommandManager.InvalidateRequerySuggested();
        }

        // =================== CRUD COMMANDS ===================
        private void ExecuteSave(bool isUpdate)
        {
            if (SelectedCustomer == null || !IsValidCustomer(SelectedCustomer)) return;

            using var context = new Sem7Prn212Context();
            var customerToSave = SelectedCustomer!;

            if (!isUpdate) // CREATE
            {
                if (context.Customers.Any(c => c.PhoneNumber == customerToSave.PhoneNumber))
                {
                    MessageBox.Show("Số điện thoại này đã tồn tại trong hệ thống!", "Lỗi trùng lặp", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                context.Customers.Add(customerToSave);
                context.SaveChanges();
                MessageBox.Show("Đã thêm khách hàng mới thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else // UPDATE
            {
                var existingCustomer = context.Customers.Find(customerToSave.CustomerId);
                if (existingCustomer == null) return;

                if (context.Customers.Any(c => c.PhoneNumber == customerToSave.PhoneNumber && c.CustomerId != customerToSave.CustomerId))
                {
                    MessageBox.Show("Số điện thoại này đã tồn tại trong hệ thống bởi khách hàng khác!", "Lỗi trùng lặp", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                existingCustomer.Name = customerToSave.Name;
                existingCustomer.Email = customerToSave.Email;
                existingCustomer.PhoneNumber = customerToSave.PhoneNumber;
                existingCustomer.Status = customerToSave.Status;

                context.SaveChanges();
                MessageBox.Show("Đã cập nhật khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadData();

            var savedCustomer = context.Customers.FirstOrDefault(c => c.PhoneNumber == customerToSave.PhoneNumber);
            SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == savedCustomer?.CustomerId);
        }

        private void ExecuteDeleteCustomer()
        {
            if (SelectedCustomer == null || SelectedCustomer.CustomerId <= 0)
            {
                MessageBox.Show("Vui lòng chọn khách hàng đã tồn tại để xóa!");
                return;
            }

            if (MessageBox.Show($"Xác nhận xóa khách hàng '{SelectedCustomer.Name}'?",
                                "Xóa Khách hàng",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            using var context = new Sem7Prn212Context();
            var customer = context.Customers.Find(SelectedCustomer.CustomerId);

            if (customer != null)
            {
                try
                {
                    context.Customers.Remove(customer);
                    context.SaveChanges();

                    LoadData();
                    ExecuteNewCustomerForm();
                    MessageBox.Show("Đã xóa khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (DbUpdateException)
                {
                    MessageBox.Show("Không thể xóa khách hàng này do có đơn hàng liên quan tồn tại trong hệ thống.",
                                    "Lỗi Xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}