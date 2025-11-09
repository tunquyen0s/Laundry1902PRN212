using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LaundryWPF.Helpers
{
    public class EmailService
    {
        private readonly string _fromEmail = "khangvm.ce191371@gmail.com";
        private readonly string _appPassword = "zcivcwqoysmkkcnu";

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_fromEmail);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true; // nếu muốn hỗ trợ HTML

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(_fromEmail, _appPassword);
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }

                System.Windows.MessageBox.Show("✅ Gửi mail thành công!");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("❌ Lỗi gửi mail: " + ex.Message);
            }
        }
    }
}
