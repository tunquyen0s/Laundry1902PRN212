using LaundryWPF.ViewModels;
using System.Windows.Controls;

namespace LaundryWPF.Views
{
    /// <summary>
    /// Interaction logic for ServiceManagementPage.xaml
    /// </summary>
    public partial class ServiceManagementPage : Page
    {
        public ServiceManagementPage()
        {
            InitializeComponent();
            DataContext = new ServiceManagementViewModel();
        }
    }
}