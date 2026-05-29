using MimeKit;
using MailKit.Net.Smtp;
using EcommerceApp.Models.Configurations;
using Microsoft.Extensions.Options;
using MailKit.Security;
using EcommerceApp.Services.Interfaces;

namespace EcommerceApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendInvoiceEmailAsync(string customerEmail, string fullName, byte[] pdfInvoice, string orderId)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress("test", customerEmail));
            emailMessage.Subject = $"Invoice for Order {orderId}";

            var body = new TextPart("plain")
            {
                Text = $"Dear {fullName},\n\nPlease find attached the invoice for your order {orderId}."
            };

            var attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(new MemoryStream(pdfInvoice)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = $"Invoice_{orderId}.pdf"
            };

            var multipart = new Multipart("mixed");
            multipart.Add(body);
            multipart.Add(attachment);

            emailMessage.Body = multipart;

            using (var client = new SmtpClient())
            {
                try
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.Auto);

                    if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
                    {
                        await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    }

                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }
            }
        }
    }
}
