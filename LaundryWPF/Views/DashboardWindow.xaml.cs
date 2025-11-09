using LaundryWPF.Models;
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

namespace LaundryWPF.Views
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        private bool isCollapsed = false;
        public DashboardWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new PageHome()); // Trang mặc định        }
        }

        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (isCollapsed)
            {
                SidebarColumn.Width = new GridLength(200);
                BtnToggle.Content = "⮜";
                isCollapsed = false;
            }
            else
            {
                SidebarColumn.Width = new GridLength(50);
                BtnToggle.Content = "⮞";
                isCollapsed = true;
            }
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PageHome());
        }

        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
                      MainFrame.Navigate(new OrderManagementPage());
        }


        private void BtnServiceManagement_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ServiceManagementPage());
        }

      


        private void BtnManageResources_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ManageResources());
        }
        private void BtnStaff_Click(object sender, RoutedEventArgs e)
        {
            Staff.StaffManageWindow StaffManageWindow = new Staff.StaffManageWindow();
            StaffManageWindow.Show();
            this.Hide();

        }
        private void BtnCustomer_Click(object sender, RoutedEventArgs e)
        {
                  MainFrame.Navigate(new CustomerManagementPage());
        }

        private void MainFrame_ContentRendered(object sender, System.EventArgs e)
        {
            // Tự động focus trang mới (nếu cần)
        }

        private void BtnOverDue_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OverDueManagementPage());
        }
    }
}   
