//using ClosedXML.Excel;
using ClosedXML.Excel;             
using LaundryWPF.Helpers;

using LaundryWPF.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;           
using Newtonsoft.Json;
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
    public class StaffViewModel : BaseViewModel
    {
        public ObservableCollection<Staff> Staffs { get; set; }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AttentCommand { get; }
        public ICommand DeAttentCommand { get; }

        public ICommand ResetDayOffCommand { get; set; }

        public ICommand ExportExcelCommand { get; }
        public StaffViewModel()
        {
            LoadDataActive();
               AddCommand = new RelayCommand(Add);
            DeleteCommand = new RelayCommand(Delete);
            UpdateCommand = new RelayCommand(Update);
            AttentCommand = new RelayCommand(Attent);
            DeAttentCommand = new RelayCommand(DeAttent);
            ResetDayOffCommand = new RelayCommand(ResetAllDayOff);
             ExportExcelCommand = new RelayCommand(ExportToExcel);
            textboxitem = new Staff();
        }

        //Tạo đối tượng lấy dữ liệu từ các TextBox
        private Staff _textboxitem;
        public Staff textboxitem
        {
           get { return _textboxitem; }
           set { _textboxitem = value; OnPropertyChanged(nameof(textboxitem)); }
            
        }
      


        //Tạo đối tượng lấy dữ liệu selecteditem của View
        private Staff _selecteditem;
        public Staff selecteditem
        {
            get { return _selecteditem; }
            set
            {
                _selecteditem = value; OnPropertyChanged(nameof(selecteditem));

                // Cập nhật các textbox nếu selecteditem có dữ liệu
                if (_selecteditem != null)
                {
                    //Gán textboxitem = Clone của _selecteditem để không bị lỗi hiển thị UI khi thay đổi dữ liệu
                    textboxitem = JsonConvert.DeserializeObject<Staff>(
                        JsonConvert.SerializeObject(_selecteditem)
                    );
                    //OnPropertyChanged(nameof(textboxitem));
                }
            }
        }

        private void LoadData()
        {
            using (var context = new Sem7Prn212Context())
            {
                Staffs = new ObservableCollection<Staff>(context.Staffs.ToList());
            }
        }

        private void LoadDataActive()
        {
            using (var context = new Sem7Prn212Context())
            {
                Staffs = new ObservableCollection<Staff>(
                    context.Staffs
                           .Where(s => s.Status == "Active")
                           .ToList()
                );
            }
        }
        //private async Task RefreshStaffAsync()
        //{
        //    using var context = new Prn212Context();
        //    var list = await context.Staff.ToListAsync();
        //    Staffs.Clear();
        //    Staffs = new ObservableCollection<Staff>(list);
        //    OnPropertyChanged(nameof(Staffs));
        //}

        private async Task RefreshStaffAsync()
        {
            using var context = new Sem7Prn212Context();
            var list = await context.Staffs
                                    .Where(s => s.Status == "Active")
                                    .ToListAsync();

            Staffs.Clear();
            foreach (var item in list)
            {
                Staffs.Add(item);
            }
        }

        private async void Add(object obj)
        {
            string name = _textboxitem.Name?.Trim();
            string phone = _textboxitem.PhoneNumber?.Trim();
            string status = "Active";
            decimal salary; 

         
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Tên không được để trống!");
                return;
            }

            
            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Số điện thoại không được để trống!");
                return;
            }

            if (!Regex.IsMatch(phone, @"^[0-9]{9,11}$"))
            {
                MessageBox.Show("Số điện thoại chỉ được chứa số và có độ dài từ 9 đến 11 chữ số!");
                return;
            }

           
            using (var context = new Sem7Prn212Context())
            {
                bool exists = context.Staffs.Any(s => s.PhoneNumber == phone);
                if (exists)
                {
                    MessageBox.Show("Số điện thoại này đã tồn tại!");
                    return;
                }
            }

           
            if (!decimal.TryParse(_textboxitem.Salary.ToString(), out salary))
            {
                MessageBox.Show("Lương phải là một số hợp lệ!");
                return;
            }

            if (salary <= 0)
            {
                MessageBox.Show("Lương phải lớn hơn 0!");
                return;
            }
            using (var context = new Sem7Prn212Context())
            {


                var newitem = new Staff
                {
                  
                    Name =  _textboxitem.Name,
                    PhoneNumber =  _textboxitem.PhoneNumber,
                    Status = "Active",
                    CreateAt = DateTime.Now,
                    Salary = _textboxitem.Salary,
                    DayOff = 0
                };

                context.Staffs.Add(newitem);
                context.SaveChanges();
            }

            await RefreshStaffAsync();
            textboxitem = new Staff();
        }

        private async void Delete(object obj)
        {
            if (_selecteditem != null)
            {
                // Hiển thị hộp thoại xác nhận
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn vô hiệu hóa nhân viên \"{_selecteditem.Name}\" không?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new Sem7Prn212Context())
                    {
                        var staff = context.Staffs.FirstOrDefault(s => s.StaffId == _selecteditem.StaffId);

                        if (staff != null)
                        {
                            staff.Status = "UnActive";
                            staff.PhoneNumber = "vo hieu hoa";

                            context.SaveChanges();
                        }
                    }

                    await RefreshStaffAsync();
                    textboxitem = new Staff();

                    MessageBox.Show("Đã vô hiệu hóa nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Đã hủy thao tác.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Attent(object obj)
        {
            if (obj is Staff selectedStaff)
            {
                using (var context = new Sem7Prn212Context())
                {
                    var staff = await context.Staffs.FirstOrDefaultAsync(s => s.StaffId == selectedStaff.StaffId);

                    if (staff != null)
                    {           
                        staff.DayOff = (staff.DayOff ?? 0) + 1;

                        await context.SaveChangesAsync();
                    }
                }

                await RefreshStaffAsync();
            }
        }

        private async void DeAttent(object obj)
        {
            if (obj is Staff selectedStaff)
            {
                using (var context = new Sem7Prn212Context())
                {
                    var staff = await context.Staffs.FirstOrDefaultAsync(s => s.StaffId == selectedStaff.StaffId);

                    if (staff != null)
                    {
                
                        int current = staff.DayOff ?? 0;

                        if (current > 0)
                        {
                            staff.DayOff = current - 1;
                            await context.SaveChangesAsync();
                        }
                     
                    }
                }

                await RefreshStaffAsync();
            }
        }


        //private async void Delete(object obj)
        //{
        //    if (_selecteditem != null)
        //    {
        //        using (var context = new Prn212Context())
        //        {
        //            context.Staff.Remove(_selecteditem);
        //            context.SaveChanges();
        //        }
        //        await RefreshStaffAsync();
        //        textboxitem = new Staff();
        //    }


        //}

        private async void Update(object obj)
        {
            if (_selecteditem == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần cập nhật!");
                return;
            }

            string name = _textboxitem.Name?.Trim();
            string phone = _textboxitem.PhoneNumber?.Trim();
            decimal salary = _textboxitem.Salary ?? 0;

            // 1️⃣ Kiểm tra tên
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Tên không được để trống!");
                return;
            }

            // 2️⃣ Kiểm tra số điện thoại
            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Số điện thoại không được để trống!");
                return;
            }

            if (!Regex.IsMatch(phone, @"^[0-9]{9,11}$"))
            {
                MessageBox.Show("Số điện thoại chỉ được chứa số và có độ dài từ 9 đến 11 chữ số!");
                return;
            }

            // 3️⃣ Kiểm tra lương
            if (salary <= 0)
            {
                MessageBox.Show("Lương phải lớn hơn 0!");
                return;
            }

            // 4️⃣ Kiểm tra trùng số điện thoại với người khác
            using (var context = new Sem7Prn212Context())
            {
                bool duplicate = context.Staffs.Any(s => s.PhoneNumber == phone && s.StaffId != _textboxitem.StaffId);
                if (duplicate)
                {
                    MessageBox.Show("Số điện thoại này đã được dùng cho nhân viên khác!");
                    return;
                }

                // 5️⃣ Cập nhật
                context.Staffs.Update(_textboxitem);
                context.SaveChanges();
            }

            MessageBox.Show("Cập nhật thông tin nhân viên thành công!");
            await RefreshStaffAsync();

            _textboxitem = new Staff(); // reset form
        }

        private async void ResetAllDayOff(object obj)
        {
            using (var context = new Sem7Prn212Context())
            {
                var allStaff = await context.Staffs.ToListAsync();

                foreach (var staff in allStaff)
                {
                    staff.DayOff = 0;
                }

                await context.SaveChangesAsync();
            }

            await RefreshStaffAsync();
        }


        private void ExportToExcel(object obj)
        {
            if (Staffs == null || Staffs.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dialog = new SaveFileDialog
                {
                    FileName = $"LuongNhanVien {DateTime.Now:MM-yyyy}.xlsx",
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx"
                };

                if (dialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Nhân viên");

                        worksheet.Cell(1, 1).Value = "Tên";
                        worksheet.Cell(1, 2).Value = "Số điện thoại";
                        worksheet.Cell(1, 3).Value = "Lương";

                        int row = 2;

                        // ✅ Chỉ lấy nhân viên có trạng thái Active
                        foreach (var staff in Staffs.Where(s => s.Status == "Active"))
                        {
                            worksheet.Cell(row, 1).Value = staff.Name;
                            worksheet.Cell(row, 2).Value = staff.PhoneNumber;
                            worksheet.Cell(row, 3).Value =
                                ((double)(staff.Salary ?? 0) / 30.0) * (30 - (staff.DayOff ?? 0));

                            row++;
                        }

                        worksheet.Columns().AdjustToContents();
                        worksheet.Row(1).Style.Font.Bold = true;
                        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

                        workbook.SaveAs(dialog.FileName);
                    }

                    MessageBox.Show("Xuất Excel thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
