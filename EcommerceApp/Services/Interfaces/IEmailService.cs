namespace EcommerceApp.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendInvoiceEmailAsync(string customerEmail, string name, byte[] pdfInvoice, string orderId);
    }
}
