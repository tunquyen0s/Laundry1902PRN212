using LaundryWPF.Helpers;
using LaundryWPF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LaundryWPF.ViewModels
{
    internal class ManageResourcesViewModel : BaseViewModel
    {
        // 🔹 Thuộc tính chính
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
                        Type = _selectedResource.Type,
                        Name = _selectedResource.Name,
                        Unit = _selectedResource.Unit,
                        Description = _selectedResource.Description,
                        PricePerUnit = _selectedResource.PricePerUnit,
                        Quantity = _selectedResource.Quantity
                    };
                }
                OnPropertyChanged(nameof(SelectedResource));
            }
        }

        private Resource _textBoxItem;
        public Resource TextBoxItem
        {
            get => _textBoxItem;
            set { _textBoxItem = value; OnPropertyChanged(nameof(TextBoxItem)); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        public ObservableCollection<string> UnitOptions { get; set; }

        // 🔹 Command
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }

        // 🔹 Constructor
        public ManageResourcesViewModel()
        {
            Load();
            TextBoxItem = new Resource();
            UnitOptions = new ObservableCollection<string> { "Kg", "Lít" };

            AddCommand = new RelayCommand(Add);
            UpdateCommand = new RelayCommand(Update);
            DeleteCommand = new RelayCommand(Delete);
            SearchCommand = new RelayCommand(Search);


            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(500); // chờ 1.5s để UI render xong
                CheckQuantityOfResources();
            }, DispatcherPriority.ContextIdle);

        }

        // 🔹 Load danh sách
        private void Load()
        {
            using (var context = new Sem7Prn212Context())
            {
                var list = context.Resources.ToList();
                Resources = new ObservableCollection<Resource>(list);
            }
        }

        private void CheckQuantityOfResources()
        {
        
            using (var context = new Sem7Prn212Context())
            {
                var lowStockResources = context.Resources
                    .Where(r => r.Quantity < 3)
                    .ToList();
                foreach (var res in lowStockResources)
                {
                    MessageBox.Show(
                        $"⚠ Tài nguyên '{res.Name}' chỉ còn {res.Quantity} {res.Unit} trong kho. Vui lòng bổ sung!",
                        "Cảnh báo tồn kho thấp", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        // 🔹 Thêm dữ liệu (async)
        private async void Add(object obj)
        {
            var newRes = new Resource
            {
                Type = TextBoxItem.Type,
                Name = TextBoxItem.Name,
                Unit = TextBoxItem.Unit,
                Description = TextBoxItem.Description,
                PricePerUnit = TextBoxItem.PricePerUnit,
                Quantity = TextBoxItem.Quantity
            };

            // Kiểm tra validation annotation
            if (!ValidateResource(newRes)) return;

            using (var context = new Sem7Prn212Context())
            {
                context.Resources.Add(newRes);
                await context.SaveChangesAsync();
            }

            await RefreshResourcesAsync();
            TextBoxItem = new Resource();
            MessageBox.Show("✅ Thêm tài nguyên thành công!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 🔹 Cập nhật dữ liệu
        private async void Update(object obj)
        {
            if (SelectedResource == null)
            {
                MessageBox.Show("⚠ Hãy chọn tài nguyên cần cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra dữ liệu trước khi cập nhật
            if (!ValidateResource(TextBoxItem)) return;

            using (var context = new Sem7Prn212Context())
            {
                context.Resources.Update(TextBoxItem);
                await context.SaveChangesAsync();
            }

            await RefreshResourcesAsync();
            TextBoxItem = new Resource();
            MessageBox.Show("🔄 Cập nhật thành công!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 🔹 Xóa dữ liệu
        private async void Delete(object obj)
        {
            if (SelectedResource == null)
            {
                MessageBox.Show("⚠ Hãy chọn tài nguyên cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa '{SelectedResource.Name}'?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            using (var context = new Sem7Prn212Context())
            {
                context.Resources.Remove(SelectedResource);
                await context.SaveChangesAsync();
            }

            await RefreshResourcesAsync();
            TextBoxItem = new Resource();
            MessageBox.Show("🗑 Xóa thành công!", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 🔹 Tìm kiếm
        private void Search(object obj)
        {
            var query = _searchText?.Trim().ToLower() ?? "";

            using var context = new Sem7Prn212Context();
            var filtered = context.Resources
                .Where(r => r.Name.ToLower().Contains(query) || r.Type.ToLower().Contains(query))
                .ToList();

            Resources.Clear();
            foreach (var item in filtered)
                Resources.Add(item);
        }

        // 🔹 Refresh danh sách
        public async Task RefreshResourcesAsync()
        {
            using var context = new Sem7Prn212Context();
            var list = await context.Resources.ToListAsync();

            Resources.Clear();
            foreach (var item in list)
                Resources.Add(item);

            OnPropertyChanged(nameof(Resources));
        }

        // 🔹 Validation Annotation
        private bool ValidateResource(Resource res)
        {
            var context = new ValidationContext(res, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(res, context, results, validateAllProperties: true);
            if (!isValid)
            {
                string errors = string.Join("\n", results.Select(r => $"• {r.ErrorMessage}"));
                MessageBox.Show(errors, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return isValid;
        }
    }
}
