using LaundryWPF.Helpers;
using LaundryWPF.Models;
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
    internal class ManageResourcesViewModel : BaseViewModel
    {
        private readonly Sem7Prn212Context _context;

        private Resource _selectedResource;
        private string _searchText;

        public ObservableCollection<Resource> Resources { get; set; }

        public Resource SelectedResource
        {
            get => _selectedResource;
            set
            {
                _selectedResource = value;
                OnPropertyChanged();

                if (value != null)
                {
                    Type = value.Type;
                    Name = value.Name;
                    Unit = value.Unit;
                    Description = value.Description;
                    PricePerUnit = value.PricePerUnit ?? 0;
                    Quantity = value.Quantity ?? 0;
                }
            }
        }

        // 🔹 Thuộc tính cho form
        private string _type;
        public string Type { get => _type; set { _type = value; OnPropertyChanged(); } }

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _unit;
        public string Unit { get => _unit; set { _unit = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private decimal _pricePerUnit;
        public decimal PricePerUnit { get => _pricePerUnit; set { _pricePerUnit = value; OnPropertyChanged(); } }

        private decimal _quantity;
        public decimal Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); } }

        public ObservableCollection<string> UnitOptions { get; set; }
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        // 🔹 Command
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        public ManageResourcesViewModel()
        {
            _context = new Sem7Prn212Context();
            Resources = new ObservableCollection<Resource>(_context.Resources.ToList());

            UnitOptions = new ObservableCollection<string> { "Kg", "Lít" };
            Unit = UnitOptions.First();
            AddCommand = new RelayCommand(_ => AddResource());
            UpdateCommand = new RelayCommand(_ => UpdateResource(), _ => SelectedResource != null);
            DeleteCommand = new RelayCommand(_ => DeleteResource(), _ => SelectedResource != null);
            ClearCommand = new RelayCommand(_ => ClearFields());
            SearchCommand = new RelayCommand(_ => SearchResource());
            RefreshCommand = new RelayCommand(_ => LoadResources());
            LoadResources();
        }

        #region CRUD logic

        private void LoadResources()
        {
            try
            {
                Resources.Clear();

                var lowStockItems = new List<string>();

                foreach (var r in _context.Resources)
                {
                    Resources.Add(r);

                    // Kiểm tra hàng sắp hết
                    if (r.Quantity.HasValue && r.Quantity.Value < 3)
                    {
                        lowStockItems.Add($"{r.Name} (SL: {r.Quantity})");
                    }
                }

                // Nếu có ít nhất 1 item sắp hết hàng
                if (lowStockItems.Any())
                {
                    string message = "⚠ Các tài nguyên sắp hết hàng:\n" +
                                     string.Join("\n", lowStockItems);
                    MessageBox.Show(message, "Cảnh báo tồn kho thấp",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi tải danh sách tài nguyên:\n{ex.Message}",
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddResource()
        {
            try
            {

                // Kiểm tra rỗng
                if (string.IsNullOrWhiteSpace(Name))
                {
                    MessageBox.Show("⚠ Vui lòng nhập tên tài nguyên.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ✅ Kiểm tra Quantity có phải số hợp lệ không
                if (!decimal.TryParse(Quantity.ToString(), out decimal qty))
                {
                    MessageBox.Show("⚠ Quantity phải là số hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ✅ Kiểm tra PricePerUnit có phải số hợp lệ không
                if (!decimal.TryParse(PricePerUnit.ToString(), out decimal price))
                {
                    MessageBox.Show("⚠ Price per unit phải là số hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ✅ Kiểm tra âm
                if (qty < 0)
                {
                    MessageBox.Show("⚠ Quantity không thể nhỏ hơn 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (price < 0)
                {
                    MessageBox.Show("⚠ Price per unit không thể nhỏ hơn 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newRes = new Resource
                {
                    Type = Type,
                    Name = Name,
                    Unit = Unit,
                    Description = Description,
                    PricePerUnit = price,
                    Quantity = qty
                };

                _context.Resources.Add(newRes);
                _context.SaveChanges();
                Resources.Add(newRes);

                MessageBox.Show("✅ Thêm tài nguyên thành công!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi thêm tài nguyên:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // 🔁 Reset lại EF Context nếu entity lỗi
                _context.ChangeTracker.Clear();
            }
        }


        private void UpdateResource()
        {
            if (SelectedResource == null)
            {
                MessageBox.Show("⚠ Hãy chọn tài nguyên cần cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 🔹 Kiểm tra dữ liệu nhập
                if (string.IsNullOrWhiteSpace(Name))
                {
                    MessageBox.Show("⚠ Vui lòng nhập tên tài nguyên.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔹 Kiểm tra số lượng hợp lệ
                if (!decimal.TryParse(Quantity.ToString(), out decimal qty))
                {
                    MessageBox.Show("⚠ Quantity phải là số hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔹 Kiểm tra giá hợp lệ
                if (!decimal.TryParse(PricePerUnit.ToString(), out decimal price))
                {
                    MessageBox.Show("⚠ Price per unit phải là số hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔹 Kiểm tra âm
                if (qty < 0)
                {
                    MessageBox.Show("⚠ Quantity không thể nhỏ hơn 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (price < 0)
                {
                    MessageBox.Show("⚠ Price per unit không thể nhỏ hơn 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔹 Lấy resource từ DB và cập nhật
                var res = _context.Resources.FirstOrDefault(r => r.ResourceId == SelectedResource.ResourceId);
                if (res != null)
                {
                    res.Type = Type;
                    res.Name = Name;
                    res.Unit = Unit;
                    res.Description = Description;
                    res.PricePerUnit = price;
                    res.Quantity = qty;

                    _context.SaveChanges();
                    LoadResources();

                    MessageBox.Show("🔄 Cập nhật thành công!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi cập nhật:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _context.ChangeTracker.Clear(); // rollback nếu lỗi EF
            }
        }


        private void DeleteResource()
        {
            if (SelectedResource == null)
            {
                MessageBox.Show("⚠ Hãy chọn tài nguyên cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Xóa tài nguyên '{SelectedResource.Name}'?",
                                          "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _context.Resources.Remove(SelectedResource);
                _context.SaveChanges();
                Resources.Remove(SelectedResource);
                ClearFields();

                MessageBox.Show("🗑 Xóa thành công!", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchResource()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadResources();
                    return;
                }

                var results = _context.Resources
                    .Where(r => r.Name.Contains(SearchText) || r.Type.Contains(SearchText))
                    .ToList();

                Resources.Clear();
                foreach (var r in results)
                    Resources.Add(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi tìm kiếm:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFields()
        {
            Type = Name = Unit = Description = string.Empty;
            PricePerUnit = 0;
            Quantity = 0;
              Unit = UnitOptions.First();
            SelectedResource = null;
        }

        #endregion
    }
}
