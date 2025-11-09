using LaundryWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LaundryWPF.Views.Staff
{
    /// <summary>
    /// Interaction logic for StaffManageWindow.xaml
    /// </summary>
    public partial class StaffManageWindow : Window
    {
        public StaffManageWindow()
        {
            InitializeComponent();
            DataContext = new StaffViewModel();
        }

        private void btnDashBoard_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow();
            dashboard.Show();
            this.Close();
            
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            StaffDataGrid.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Content = new add();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            StaffDataGrid.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Content = new update();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            StaffDataGrid.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Content = new delete();
        }

        private void btnAttent_Click(object sender, RoutedEventArgs e)
        {
            StaffDataGrid.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Content = new attent();
        }
    }
}
