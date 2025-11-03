//using ClosedXML.Excel;
using LaundryWPF.Helpers;
using LaundryWPF.Models;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClosedXML.Excel;             
using Microsoft.Win32;            
using System.Windows;              
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
            using (var context = new Prn212Context())
            {
                Staffs = new ObservableCollection<Staff>(context.Staff.ToList());
            }
        }

        private void LoadDataActive()
        {
            using (var context = new Prn212Context())
            {
                Staffs = new ObservableCollection<Staff>(
                    context.Staff
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
            using var context = new Prn212Context();
            var list = await context.Staff
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
            //if (string.IsNullOrWhiteSpace(_textboxitem.Name))
            //{
            //    MessageBox.Show("Tên nhân viên không được để trống!");
            //    return;
            //}

            //if (_textboxitem.Name.Length > 100)
            //{
            //    MessageBox.Show("Tên nhân viên không được vượt quá 100 ký tự!");
            //    return;
            //}

            //if (string.IsNullOrWhiteSpace(_textboxitem.PhoneNumber))
            //{
            //    MessageBox.Show("Số điện thoại không được để trống!");
            //    return;
            //}

            //if (_textboxitem.PhoneNumber.Length > 20)
            //{
            //    MessageBox.Show("Số điện thoại không được vượt quá 20 ký tự!");
            //    return;
            //}

            //if (_textboxitem.PhoneNumber.Any(c => c > 127))
            //{
            //    MessageBox.Show("Số điện thoại chỉ được chứa ký tự ASCII (không dấu)!");
            //    return;
            //}

            using (var context = new Prn212Context())
            {
                bool exists = context.Staff.Any(s => s.PhoneNumber == _textboxitem.PhoneNumber);
                if (exists)
                {
                    MessageBox.Show("Số điện thoại này đã tồn tại!");
                    return;
                }

                var newitem = new Staff
                {
                    
                    Name = _textboxitem.Name,
                    PhoneNumber = _textboxitem.PhoneNumber,
                    Status = "Active",
                    CreateAt = DateTime.Now,
                    Salary = _textboxitem.Salary,
                    DayOff = 0
                };

                context.Staff.Add(newitem);
                context.SaveChanges();
            }

            await RefreshStaffAsync();
            textboxitem = new Staff();
        }

        private async void Delete(object obj)
        {
            if (_selecteditem != null)
            {
                using (var context = new Prn212Context())
                {
              
                    var staff = context.Staff.FirstOrDefault(s => s.StaffId == _selecteditem.StaffId);

                    if (staff != null)
                    {
                        
                        staff.Status = "UnActive";

                        context.SaveChanges();
                    }
                }

                await RefreshStaffAsync();

                textboxitem = new Staff();
            }


        }

        private async void Attent(object obj)
        {
            if (obj is Staff selectedStaff)
            {
                using (var context = new Prn212Context())
                {
                    var staff = await context.Staff.FirstOrDefaultAsync(s => s.StaffId == selectedStaff.StaffId);

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
                using (var context = new Prn212Context())
                {
                    var staff = await context.Staff.FirstOrDefaultAsync(s => s.StaffId == selectedStaff.StaffId);

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
            if (_selecteditem != null)
            {
                using (var context = new Prn212Context())
                {
                    context.Staff.Update(_textboxitem);
                    context.SaveChanges();
                }
                await RefreshStaffAsync();

                textboxitem = new Staff();
            }
        }

        private async void ResetAllDayOff(object obj)
        {
            using (var context = new Prn212Context())
            {
                var allStaff = await context.Staff.ToListAsync();

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
                    FileName = "LuongNhanVien.xlsx",
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
                        foreach (var staff in Staffs)
                        {
                            worksheet.Cell(row, 1).Value = staff.Name;
                            worksheet.Cell(row, 2).Value = staff.PhoneNumber;
                            worksheet.Cell(row, 3).Value = (double)staff.Salary / 30 * (30 - staff.DayOff);

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
