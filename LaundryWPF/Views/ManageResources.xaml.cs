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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaundryWPF.Views
{
    /// <summary>
    /// Interaction logic for ManageResources.xaml
    /// </summary>
    public partial class ManageResources : Page
    {
        public ManageResources()
        {
            InitializeComponent();
            DataContext = new ManageResourcesViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
