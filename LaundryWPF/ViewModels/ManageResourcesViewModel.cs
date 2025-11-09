using DocumentFormat.OpenXml.InkML;
using LaundryWPF.Helpers;
using LaundryWPF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LaundryWPF.ViewModels
{
    internal class ManageResourcesViewModel : BaseViewModel
    {
        private readonly Sem7Prn212Context _context;

        public ObservableCollection<Resource> Resources { get; set; }


        private Resource _selectedResource;
        public Resource SelectedResource
        {
            get => _selectedResource;
            set
            {

                _selectedResource = value;

                if (_selectedResource != null)
                {


                    TextBoxItem = new Resource
                    {
                        ResourceId = _selectedResource.ResourceId,
                        Name = _selectedResource.Name,
                        Type = _selectedResource.Type,
                        Unit = _selectedResource.Unit,
                        Description = _selectedResource.Description,
                        Quantity = _selectedResource.Quantity,
                        PricePerUnit = _selectedResource.PricePerUnit,
                    };
                }


                OnPropertyChanged(nameof(SelectedResource));
            }
        }


        private Resource _textBoxItem;
        public Resource TextBoxItem
        {
            get => _textBoxItem;
            set
            {

                _textBoxItem = value;

                OnPropertyChanged(nameof(TextBoxItem));
            }
        }



        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {

                _searchText = value;


                OnPropertyChanged(nameof(SearchText));
            }
        }


        public ObservableCollection<string> UnitOptions { get; set; }

        // 🔹 Command
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand SearchCommand { get; }

        public ManageResourcesViewModel()
        {
            _context = new Sem7Prn212Context();
            Resources = new ObservableCollection<Resource>(_context.Resources.ToList());

            UnitOptions = new ObservableCollection<string> { "kg", "lít", "cái", "m³" };
            TextBoxItem = new Resource
            {
                Unit = UnitOptions.FirstOrDefault()
            };
            AddCommand = new RelayCommand(AddResource);
            UpdateCommand = new RelayCommand(UpdateResource);
            DeleteCommand = new RelayCommand(DeleteResource);
            ClearCommand = new RelayCommand(ClearFields);
            SearchCommand = new RelayCommand(SearchResource);
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


        private async void AddResource(Object obj)
        {
            try
            {  
                String name = TextBoxItem.Name;
                var quantity = TextBoxItem.Quantity.ToString();
                String type = TextBoxItem.Type;
                var PricePerUnit = TextBoxItem.PricePerUnit.ToString();
                if (name == null || name.Trim().Length == 0)
                {
                    MessageBox.Show("⚠ Vui lòng nhập tên tài nguyên.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (type == null || type.Trim().Length == 0)
                {
                    MessageBox.Show("⚠ Vui lòng nhập loại tài nguyên.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!Decimal.TryParse(quantity, out _))
                {
                    MessageBox.Show("⚠ Số lượng tài nguyên phải là một số hợp lệ.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Decimal.Parse(quantity) < 0)
                {
                    MessageBox.Show("⚠ Số lượng tài nguyên không thể nhỏ hơn 0.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if(!Decimal.TryParse(PricePerUnit, out _))
                {
                    MessageBox.Show("⚠ Giá tài nguyên phải là một số hợp lệ.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Decimal.Parse(PricePerUnit) < 0)
                {
                    MessageBox.Show("⚠ Giá tài nguyên không thể nhỏ hơn 0.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newRes = new Resource
                {
                    Type = type,
                    Name = name,
                    Unit = TextBoxItem.Unit,
                    Description = TextBoxItem.Description,
                    PricePerUnit = Decimal.Parse(PricePerUnit),
                    Quantity = Decimal.Parse(quantity)
                };

                _context.Resources.Add(newRes);
                _context.SaveChanges();
                Resources.Add(newRes);
                TextBoxItem = new Resource
                {
                    Unit = UnitOptions.FirstOrDefault(),
                };
                MessageBox.Show("✅ Thêm tài nguyên thành công!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi thêm tài nguyên:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // 🔁 Reset lại EF Context nếu entity lỗi
                _context.ChangeTracker.Clear();
            }
        }


        private async void UpdateResource(Object obj)
        {
            if (SelectedResource == null)
            {
                MessageBox.Show("⚠ Hãy chọn tài nguyên cần cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                String name = TextBoxItem.Name;
                var quantity = TextBoxItem.Quantity.ToString();
                String type = TextBoxItem.Type;
                var PricePerUnit = TextBoxItem.PricePerUnit.ToString();
                if (name == null || name.Trim().Length == 0)
                {
                    MessageBox.Show("⚠ Vui lòng nhập tên tài nguyên.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (type == null || type.Trim().Length == 0)
                {
                    MessageBox.Show("⚠ Vui lòng nhập loại tài nguyên.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!Decimal.TryParse(quantity, out _))
                {
                    MessageBox.Show("⚠ Số lượng tài nguyên phải là một số hợp lệ.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Decimal.Parse(quantity) < 0)
                {
                    MessageBox.Show("⚠ Số lượng tài nguyên không thể nhỏ hơn 0.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!Decimal.TryParse(PricePerUnit, out _))
                {
                    MessageBox.Show("⚠ Giá tài nguyên phải là một số hợp lệ.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Decimal.Parse(PricePerUnit) < 0)
                {
                    MessageBox.Show("⚠ Giá tài nguyên không thể nhỏ hơn 0.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var context = new Sem7Prn212Context();  
                // 🔹 Lấy resource từ DB và cập nhật
                context.Update(TextBoxItem);
                await context.SaveChangesAsync();
                await RefreshAsync();
                TextBoxItem = new Resource
                {
                    Unit = UnitOptions.FirstOrDefault(),
                };
                MessageBox.Show("🔄 Cập nhật thành công!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi cập nhật:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _context.ChangeTracker.Clear(); // rollback nếu lỗi EF
            }
        }


        private async void DeleteResource(Object obj)
        {
            if (SelectedResource == null)
            {
                MessageBox.Show("⚠ Hãy chọn tài nguyên cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Xóa tài nguyên '{TextBoxItem.Name}'?",
                                          "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var context = new Sem7Prn212Context();
                context.Resources.Remove(SelectedResource);
                await context.SaveChangesAsync();
                await RefreshAsync();

                TextBoxItem = new Resource
                {
                    Unit = UnitOptions.FirstOrDefault(),
                };
                MessageBox.Show("🗑 Xóa thành công!", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi xóa:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchResource(object obj)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadResources();
                    return;
                }

                var keyword = _searchText?.Trim().ToLower() ?? "";
                using var context = new Sem7Prn212Context();
                var filtered = context.Resources
                    .Where(w =>
                        w.Name.ToLower().Contains(keyword) ||
                        w.Type.ToLower().Contains(keyword))
                    .ToList();

                Resources.Clear();
                foreach (var w in filtered)
                    Resources.Add(w);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi tìm kiếm:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClearFields(object obj)
        {                   
            TextBoxItem = new Resource
            {
                Unit = UnitOptions.FirstOrDefault(),
            };
            SelectedResource = null;
        }

        private async Task RefreshAsync()
        {
            using var context = new Sem7Prn212Context();

            var list = await context.Resources.ToListAsync();

            Resources.Clear();
            foreach (var item in list)
                Resources.Add(item);

            OnPropertyChanged(nameof(Resources));
        }

        #endregion
    }
}
