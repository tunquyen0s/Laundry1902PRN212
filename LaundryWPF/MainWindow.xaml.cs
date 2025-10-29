using LaundryWPF.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaundryWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenLaundryWindow_Click(object sender, RoutedEventArgs e)
        {
            string pin = PinBox.Password;
            if (pin != "1234")
            {
                MessageBox.Show("Mã PIN không đúng. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                PinBox.Clear();
                PinBox.Focus();
            }
            else
            {
                var lw = new DashboardWindow();
                lw.Show();
                this.Close();
            }

        }
    }
}