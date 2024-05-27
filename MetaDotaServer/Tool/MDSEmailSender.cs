using MailKit.Net.Smtp;
using MimeKit;
using System.Net;

namespace MetaDotaServer.Tool
{
    public class MDSEmailSender
    {
        private readonly IConfiguration _configuration;

        private string _myEmail;

        private string _myPassword;

        private string _mySmtpHost;

        private int _mySmtpPort;

        public MDSEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
            _myEmail = _configuration["Email:MyEmail"];
            _myPassword = _configuration["Email:Password"];
            _mySmtpHost = _configuration["Email:Smtp"];
            _mySmtpPort = int.Parse(_configuration["Email:Port"]);
        }


        public async Task<bool> Send(string toEmail, string subject, string body)
        {
            try
            {
                // 创建邮件对象
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("suanqtang", _myEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                // 邮件正文
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = body;
                message.Body = bodyBuilder.ToMessageBody();


                using (var client = new SmtpClient())
                {
                    // 连接到SMTP服务器
                    client.Connect(_mySmtpHost, _mySmtpPort, true);

                    // 使用邮箱和密码进行身份验证
                    client.Authenticate(_myEmail, _myPassword);

                    // 发送邮件
                    client.Send(message);

                    // 断开连接
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
