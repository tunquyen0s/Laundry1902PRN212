using LaundryWPF.Helpers;
using LaundryWPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LaundryWPF.ViewModels
{
    public class ServiceManagementViewModel : BaseViewModel
    {
        // =================== DỮ LIỆU NGUỒN ===================
        public ObservableCollection<Service> Services { get; set; }

        // =================== SERVICE ĐANG CHỌN ===================
        private Service _selectedService;
        public Service SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
                if (value != null) MapServiceToForm();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // =================== FORM PROPERTIES ===================
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); ValidateField(nameof(Name)); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); ValidateField(nameof(Description)); }
        }

        private decimal? _pricePerUnit;
        public decimal? PricePerUnit
        {
            get => _pricePerUnit;
            set { _pricePerUnit = value; OnPropertyChanged(); ValidateField(nameof(PricePerUnit)); }
        }

        private int? _timeCost;
        public int? TimeCost
        {
            get => _timeCost;
            set { _timeCost = value; OnPropertyChanged(); ValidateField(nameof(TimeCost)); }
        }

        // =================== VALIDATION ERROR PROPERTIES ===================
        private string _nameError = string.Empty;
        public string NameError
        {
            get => _nameError;
            set { _nameError = value; OnPropertyChanged(); }
        }

        private string _descriptionError = string.Empty;
        public string DescriptionError
        {
            get => _descriptionError;
            set { _descriptionError = value; OnPropertyChanged(); }
        }

        private string _pricePerUnitError = string.Empty;
        public string PricePerUnitError
        {
            get => _pricePerUnitError;
            set { _pricePerUnitError = value; OnPropertyChanged(); }
        }

        private string _timeCostError = string.Empty;
        public string TimeCostError
        {
            get => _timeCostError;
            set { _timeCostError = value; OnPropertyChanged(); }
        }

        private string _generalError = string.Empty;
        public string GeneralError
        {
            get => _generalError;
            set { _generalError = value; OnPropertyChanged(); }
        }

        // =================== SEARCH PROPERTIES ===================
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                SearchServices();
            }
        }

        // =================== UI STATE ===================
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // =================== COMMANDS ===================
        public ICommand CreateServiceCommand { get; }
        public ICommand SaveServiceCommand { get; }
        public ICommand UpdateServiceCommand { get; }
        public ICommand DeleteServiceCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand SearchCommand { get; }

        // =================== CONSTRUCTOR ===================
        public ServiceManagementViewModel()
        {
            Services = new ObservableCollection<Service>();

            CreateServiceCommand = new RelayCommand(_ => ExecuteCreateService());
            SaveServiceCommand = new RelayCommand(_ => ExecuteSaveService());
            UpdateServiceCommand = new RelayCommand(_ => ExecuteUpdateService(), _ => CanUpdateService());
            DeleteServiceCommand = new RelayCommand(_ => ExecuteDeleteService(), _ => CanDeleteService());
            ClearFormCommand = new RelayCommand(_ => ClearForm());
            SearchCommand = new RelayCommand(_ => SearchServices());

            InitializeData();
        }

        // =================== KHỞI TẠO DỮ LIỆU ===================
        private void InitializeData()
        {
            try
            {
                using var context = new Sem7Prn212Context();
                Service.SeedSampleData(context);
                LoadServices();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo dữ liệu: {ex.Message}",
                    "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =================== LOAD DỮ LIỆU ===================
        private void LoadServices()
        {
            try
            {
                StatusMessage = "Đang tải dữ liệu...";
                using var context = new Sem7Prn212Context();

                var services = context.Services.ToList();
                Services.Clear();
                foreach (var service in services)
                    Services.Add(service);

                StatusMessage = $"Đã tải {services.Count} dịch vụ.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}",
                    "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =================== MAP / CLEAR FORM ===================
        private void MapServiceToForm()
        {
            if (SelectedService == null)
            {
                ClearForm();
                return;
            }

            Name = SelectedService.Name;
            Description = SelectedService.Description;
            PricePerUnit = SelectedService.PricePerUnit;
            TimeCost = SelectedService.TimeCost;
            ClearValidationErrors();
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Description = string.Empty;
            PricePerUnit = null;
            TimeCost = null;
            SelectedService = null;
            ClearValidationErrors();
        }

        // =================== VALIDATION ===================
        private void ClearValidationErrors()
        {
            NameError = string.Empty;
            DescriptionError = string.Empty;
            PricePerUnitError = string.Empty;
            TimeCostError = string.Empty;
            GeneralError = string.Empty;
        }

        private void ValidateField(string fieldName)
        {
            try
            {
                var temp = Service.CreateService(Name ?? "", Description ?? "", PricePerUnit ?? 0, TimeCost ?? 0);
                var errors = temp.GetValidationErrors();
                
                // Clear all errors first
                ClearValidationErrors();
                
                // Parse errors and assign to appropriate fields
                foreach (var error in errors)
                {
                    var errorMessage = error.Replace("• ", "");
                    
                    if (errorMessage.Contains("Tên dịch vụ"))
                        NameError = errorMessage;
                    else if (errorMessage.Contains("Mô tả"))
                        DescriptionError = errorMessage;
                    else if (errorMessage.Contains("Giá dịch vụ"))
                        PricePerUnitError = errorMessage;
                    else if (errorMessage.Contains("Thời gian"))
                        TimeCostError = errorMessage;
                    else
                        GeneralError = errorMessage;
                }
            }
            catch (Exception ex)
            {
                GeneralError = $"Lỗi validation: {ex.Message}";
            }
        }

        private bool CanUpdateService() =>
            SelectedService != null && SelectedService.ServiceId > 0;

        private bool CanDeleteService() =>
            SelectedService != null && SelectedService.ServiceId > 0;

        private bool HasValidationErrors()
        {
            ValidateField("All");
            return !string.IsNullOrEmpty(NameError) || 
                   !string.IsNullOrEmpty(DescriptionError) || 
                   !string.IsNullOrEmpty(PricePerUnitError) || 
                   !string.IsNullOrEmpty(TimeCostError) ||
                   !string.IsNullOrEmpty(GeneralError);
        }

        private string GetValidationErrorMessage()
        {
            try
            {
                var temp = Service.CreateService(Name ?? "", Description ?? "", PricePerUnit ?? 0, TimeCost ?? 0);
                var errors = temp.GetValidationErrors();
                return errors.Any() ? string.Join("\n", errors) : string.Empty;
            }
            catch (Exception ex)
            {
                return $"Lỗi validation: {ex.Message}";
            }
        }

        // =================== CRUD OPERATIONS ===================
        private void ExecuteCreateService()
        {
            ClearForm();
            MessageBox.Show("Tạo dịch vụ mới. Vui lòng nhập thông tin và nhấn Lưu.",
                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteSaveService()
        {
            if (HasValidationErrors())
            {
                GeneralError = "Vui lòng sửa các lỗi trước khi lưu.";
                return;
            }

            try
            {
                using var context = new Sem7Prn212Context();

                if (SelectedService == null || SelectedService.ServiceId == 0)
                {
                    var newService = Service.CreateService(Name, Description ?? "", PricePerUnit ?? 0, TimeCost ?? 0);
                    context.Services.Add(newService);
                    context.SaveChanges();

                    LoadServices();
                    SelectedService = Services.FirstOrDefault(s => s.ServiceId == newService.ServiceId);

                    GeneralError = string.Empty;
                    StatusMessage = "Đã tạo dịch vụ mới thành công.";
                }
                else ExecuteUpdateService();
            }
            catch (Exception ex)
            {
                GeneralError = $"Lỗi khi lưu dịch vụ: {ex.Message}";
            }
        }

        private void ExecuteUpdateService()
        {
            if (HasValidationErrors())
            {
                GeneralError = "Vui lòng sửa các lỗi trước khi cập nhật.";
                return;
            }

            try
            {
                using var context = new Sem7Prn212Context();
                var service = context.Services.FirstOrDefault(s => s.ServiceId == SelectedService.ServiceId);
                if (service == null)
                {
                    GeneralError = "Không tìm thấy dịch vụ.";
                    return;
                }

                service.Name = Name;
                service.Description = Description;
                service.PricePerUnit = PricePerUnit;
                service.TimeCost = TimeCost;

                context.SaveChanges();
                LoadServices();
                SelectedService = Services.FirstOrDefault(s => s.ServiceId == service.ServiceId);

                GeneralError = string.Empty;
                StatusMessage = "Đã cập nhật dịch vụ thành công.";
            }
            catch (Exception ex)
            {
                GeneralError = $"Lỗi khi cập nhật dịch vụ: {ex.Message}";
            }
        }

        private void ExecuteDeleteService()
        {
            if (!CanDeleteService())
            {
                MessageBox.Show("Vui lòng chọn dịch vụ để xóa.",
                    "Chưa chọn dịch vụ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa dịch vụ '{SelectedService.Name}'?\n\n" +
                "Lưu ý: Việc xóa dịch vụ có thể ảnh hưởng đến các đơn hàng đã sử dụng dịch vụ này.",
                "Xác nhận xóa dịch vụ", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            try
            {
                using var context = new Sem7Prn212Context();

                var hasOrders = context.Orders.Any(o => o.ServiceId == SelectedService.ServiceId);
                if (hasOrders)
                {
                    MessageBox.Show("Không thể xóa dịch vụ này vì đã có đơn hàng sử dụng.",
                        "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var service = context.Services.FirstOrDefault(s => s.ServiceId == SelectedService.ServiceId);
                if (service != null)
                {
                    context.Services.Remove(service);
                    context.SaveChanges();

                    LoadServices();
                    ClearForm();
                    MessageBox.Show("Đã xóa dịch vụ thành công.",
                        "Xóa thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa dịch vụ: {ex.Message}",
                    "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =================== SEARCH ===================
        private void SearchServices()
        {
            try
            {
                using var context = new Sem7Prn212Context();
                var query = context.Services.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    query = query.Where(s => s.Name.Contains(SearchText) ||
                                             (s.Description != null && s.Description.Contains(SearchText)));

                var results = query.ToList();
                Services.Clear();
                foreach (var item in results)
                    Services.Add(item);

                StatusMessage = $"{results.Count} kết quả tìm thấy.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}",
                    "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
