using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace LaundryWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Kiểm tra nếu có console (khi chạy debug kiểu Console)
            try
            {
                if (Environment.UserInteractive)
                {
                    Console.OutputEncoding = Encoding.UTF8;
                }
            }
            catch
            {
                // Bỏ qua nếu không có console (WPF app)
            }

            // Thiết lập ngôn ngữ mặc định là tiếng Việt cho toàn UI
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage("vi-VN"))
            );
        }
    }
}
