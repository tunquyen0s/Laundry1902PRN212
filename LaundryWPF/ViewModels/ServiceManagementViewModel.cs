using LaundryWPF.Helpers; 
using LaundryWPF.Models; 
using System.Collections.ObjectModel; 
using System.Windows; 
using System.Windows.Input;

namespace LaundryWPF.ViewModels
{
    public class ServiceManagementViewModel : BaseViewModel
    {
        public ObservableCollection<Service> Services { get; set; } = new();

        private Service? _selectedService;

        // Giao tiếp hai chiều với dòng được chọn trên DataGrid
        public Service? SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
                MapServiceToForm();
                CommandManager.InvalidateRequerySuggested();
            }
        }


        // Các thuộc tính này dùng để liên kết (binding) hai chiều với các trường nhập liệu (TextBox) trên View.
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            // Mỗi khi giá trị thay đổi, tự động gọi OnPropertyChanged và ValidateForm
            set { _name = value; OnPropertyChanged(); ValidateForm(); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); ValidateForm(); }
        }

        private decimal? _pricePerUnit;
        public decimal? PricePerUnit
        {
            get => _pricePerUnit;
            set { _pricePerUnit = value; OnPropertyChanged(); ValidateForm(); }
        }

        private int? _timeCost;
        public int? TimeCost
        {
            get => _timeCost;
            set { _timeCost = value; OnPropertyChanged(); ValidateForm(); }
        }

        // Các thuộc tính này dùng để hiển thị thông báo lỗi (Error Messages) trên View.
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

        private string _priceError = string.Empty;
        public string PriceError
        {
            get => _priceError;
            set { _priceError = value; OnPropertyChanged(); }
        }

        private string _timeError = string.Empty;
        public string TimeError
        {
            get => _timeError;
            set { _timeError = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                SearchServices(); // Tự động tìm kiếm ngay khi người dùng gõ
            }
        }

        // Thông báo trạng thái (ví dụ: "Đã tải 10 dịch vụ", "Đã lưu thành công")
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Định nghĩa các lệnh (Command) để liên kết với các nút bấm trên View (MVVM Pattern)
        public ICommand CreateServiceCommand { get; }
        public ICommand SaveServiceCommand { get; }
        public ICommand DeleteServiceCommand { get; }


        public ServiceManagementViewModel()
        {
            CreateServiceCommand = new RelayCommand(_ => CreateNewService());
            SaveServiceCommand = new RelayCommand(_ => SaveService(), _ => CanSave());
            DeleteServiceCommand = new RelayCommand(_ => DeleteService(), _ => CanDelete());

            InitializeData(); // Khởi tạo dữ liệu ban đầu
        }


        // Phương thức khởi tạo dữ liệu ban đầu cho ViewModel
        private void InitializeData()
        {
            try
            {
                using var context = new Sem7Prn212Context();
                Service.SeedSampleData(context);
                LoadServices(); // Tải dữ liệu dịch vụ vào ObservableCollection
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khởi tạo: {ex.Message}");
            }
        }

        // Tải danh sách dịch vụ từ DB và cập nhật Services (ObservableCollection)
        private void LoadServices()
        {
            try
            {
                StatusMessage = "Đang tải...";
                using var context = new Sem7Prn212Context();
                var services = context.Services.OrderBy(s => s.Name).ToList();
                Services.Clear();
                // Dùng ForEach để thêm từng item vào Services (quan trọng cho ObservableCollection)
                services.ForEach(Services.Add);
                StatusMessage = $"Đã tải {services.Count} dịch vụ.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
                ShowError($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        // Ánh xạ dữ liệu từ SelectedService vào các thuộc tính của Form
        private void MapServiceToForm()
        {
            if (SelectedService == null)
            {
                Name = string.Empty;
                Description = string.Empty;
                PricePerUnit = null;
                TimeCost = null;
                ClearErrors();
                StatusMessage = "Sẵn sàng tạo dịch vụ mới.";
                return;
            }

            Name = SelectedService.Name;
            Description = SelectedService.Description ?? string.Empty;
            PricePerUnit = SelectedService.PricePerUnit;
            TimeCost = SelectedService.TimeCost;
            ClearErrors(); // Xóa mọi lỗi khi chuyển sang dịch vụ khác
        }

        // Tạo dịch vụ mới
        private void CreateNewService()
        {
            SelectedService = null;
            Name = string.Empty;
            Description = string.Empty;
            PricePerUnit = null;
            TimeCost = null;
            ClearErrors();
            StatusMessage = "Sẵn sàng tạo dịch vụ mới.";
        }

        // Xóa tất cả các thông báo lỗi hiển thị trên View
        private void ClearErrors()
        {
            NameError = string.Empty;
            DescriptionError = string.Empty;
            PriceError = string.Empty;
            TimeError = string.Empty;
        }
        private void ValidateForm()
        {
            // Nếu form hoàn toàn rỗng, xóa lỗi và thoát (không cần validation)
            if (IsFormEmpty()) 
            {
                ClearErrors();
                return;
            }

            try
            {
                // Tạm thời tạo một đối tượng Service (sử dụng CreateService đã chuẩn hóa dữ liệu)
                var temp = Service.CreateService(Name, Description, PricePerUnit ?? 0, TimeCost ?? 0);
                // Lấy danh sách lỗi Data Annotation từ đối tượng Service
                var errors = temp.GetValidationErrors();
                
                ClearErrors();
                
                // Phân tích và gán lỗi vào các thuộc tính lỗi tương ứng
                foreach (var error in errors)
                {
                    var message = error.Replace("• ", ""); // Loại bỏ ký hiệu bullet point
                    
                    // Logic phân loại lỗi dựa trên nội dung thông báo (phải đồng bộ với Data Annotation)
                    if (message.Contains("Tên dịch vụ"))
                        NameError = message;
                    else if (message.Contains("Mô tả"))
                        DescriptionError = message;
                    else if (message.Contains("Giá dịch vụ"))
                        PriceError = message;
                    else if (message.Contains("Thời gian"))
                        TimeError = message;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi validation: {ex.Message}";
            }
        }

        // Kiểm tra xem tất cả các trường quan trọng có rỗng không
        private bool IsFormEmpty() => 
            string.IsNullOrWhiteSpace(Name) && PricePerUnit == null && TimeCost == null;

        // Kiểm tra xem có bất kỳ lỗi nào đang hiển thị hay không
        private bool HasErrors() =>
            !string.IsNullOrEmpty(NameError) || 
            !string.IsNullOrEmpty(DescriptionError) ||
            !string.IsNullOrEmpty(PriceError) || 
            !string.IsNullOrEmpty(TimeError);

        // Logic để quyết định nút LƯU có thể click được hay không (CanExecute)
        private bool CanSave() => !HasErrors() && !string.IsNullOrWhiteSpace(Name);

        // Logic để quyết định nút XÓA có thể click được hay không (CanExecute)
        private bool CanDelete() => SelectedService?.ServiceId > 0;

        // Phương thức xử lý logic Lưu (Tạo mới hoặc Cập nhật)
        private void SaveService()
        {
            ValidateForm(); // Xác thực lại lần cuối trước khi lưu
            if (HasErrors())
            {
                StatusMessage = "Vui lòng sửa lỗi trước khi lưu.";
                return;
            }

            try
            {
                using var context = new Sem7Prn212Context();

                if (IsNewService())
                    SaveNewService(context);
                else
                    UpdateExistingService(context);
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi lưu: {ex.Message}");
            }
        }

        // Kiểm tra xem đây là thao tác Tạo mới hay Cập nhật
        private bool IsNewService() => SelectedService == null || SelectedService.ServiceId == 0;

        // Thao tác Tạo mới dịch vụ
        private void SaveNewService(Sem7Prn212Context context)
        {
            // Tạo đối tượng Service mới đã được chuẩn hóa
            var newService = Service.CreateService(Name, Description, PricePerUnit ?? 0, TimeCost ?? 0);
            context.Services.Add(newService);
            context.SaveChanges();

            LoadServices(); // Tải lại danh sách
            // Chọn dịch vụ vừa tạo xong trên DataGrid
            SelectedService = Services.FirstOrDefault(s => s.ServiceId == newService.ServiceId);
            StatusMessage = "Tạo dịch vụ mới thành công.";
        }

        // Thao tác Cập nhật dịch vụ đã có
        private void UpdateExistingService(Sem7Prn212Context context)
        {
            // Tìm dịch vụ trong DB để cập nhật (Entity Framework)
            var service = context.Services.FirstOrDefault(s => s.ServiceId == SelectedService!.ServiceId);
            if (service == null)
            {
                ShowError("Không tìm thấy dịch vụ.");
                return;
            }

            // Gán lại các thuộc tính từ Form vào đối tượng DB
            service.Name = Name;
            // Chuẩn hóa Description: gán null nếu rỗng/chỉ có khoảng trắng
            service.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
            service.PricePerUnit = PricePerUnit;
            service.TimeCost = TimeCost;

            context.SaveChanges();
            LoadServices(); // Tải lại danh sách
            // Chọn lại dịch vụ vừa cập nhật trên DataGrid
            SelectedService = Services.FirstOrDefault(s => s.ServiceId == service.ServiceId);
            StatusMessage = "Cập nhật thành công.";
        }

        // Phương thức xử lý logic Xóa
        private void DeleteService()
        {
            if (!CanDelete() || !ConfirmDelete()) return; // Kiểm tra quyền xóa và xác nhận
            try
            {
                using var context = new Sem7Prn212Context();
                // Kiểm tra ràng buộc (Foreign Key Check)
                var hasOrders = context.Orders.Any(o => o.ServiceId == SelectedService!.ServiceId);
                if (hasOrders)
                {
                    ShowError("Không thể xóa dịch vụ đã có đơn hàng sử dụng.");
                    return;
                }

                var service = context.Services.Find(SelectedService.ServiceId);
                if (service != null)
                {
                    context.Services.Remove(service);
                    context.SaveChanges();
                    LoadServices();
                    SelectedService = null;
                    StatusMessage = "Đã xóa dịch vụ thành công.";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi xóa: {ex.Message}");
            }
        }

        // Hiển thị hộp thoại xác nhận xóa
        private bool ConfirmDelete()
        {
            var result = MessageBox.Show(
                $"Xóa dịch vụ '{SelectedService!.Name}'?\n\nCảnh báo: Có thể ảnh hưởng đến đơn hàng liên quan.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        // Thực hiện logic tìm kiếm và cập nhật danh sách hiển thị
        private void SearchServices()
        {
            try
            {
                using var context = new Sem7Prn212Context();
                var query = context.Services.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchTerm = SearchText.Trim().ToLower();
                    // Lọc theo Tên hoặc Mô tả (không phân biệt chữ hoa/thường)
                    query = query.Where(s => 
                        s.Name.ToLower().Contains(searchTerm) ||
                        (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
                }

                var results = query.OrderBy(s => s.Name).ToList();
                Services.Clear();
                results.ForEach(Services.Add); // Cập nhật ObservableCollection

                // Cập nhật thông báo trạng thái tìm kiếm
                StatusMessage = string.IsNullOrWhiteSpace(SearchText) 
                    ? $"Hiển thị {results.Count} dịch vụ." 
                    : $"Tìm thấy {results.Count} kết quả cho '{SearchText}'.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tìm kiếm: {ex.Message}";
                ShowError($"Lỗi tìm kiếm: {ex.Message}");
            }
        }

        private void ShowError(string message) => 
            MessageBox.Show(message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

    }
}