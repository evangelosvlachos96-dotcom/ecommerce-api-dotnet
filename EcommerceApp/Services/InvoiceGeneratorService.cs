using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using EcommerceApp.Models.Database;
using EcommerceApp.Services.Interfaces;

namespace EcommerceApp.Services
{
    public class InvoiceGeneratorService : IInvoiceGeneratorService
    {
        public string GenerateInvoice(Order order)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var graphics = XGraphics.FromPdfPage(page);

            var font = new XFont("Verdana", 20, XFontStyle.Bold);

            graphics.DrawString("Invoice", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.TopCenter);
            font = new XFont("Verdana", 12, XFontStyle.Regular);

            graphics.DrawString($"Order ID: {order.Id}", font, XBrushes.Black, new XRect(40, 100, page.Width, page.Height), XStringFormats.TopLeft);
            graphics.DrawString($"User ID: {order.UserUid}", font, XBrushes.Black, new XRect(40, 120, page.Width, page.Height), XStringFormats.TopLeft);
            graphics.DrawString($"Order Date: {order.CreatedAt}", font, XBrushes.Black, new XRect(40, 140, page.Width, page.Height), XStringFormats.TopLeft);

            graphics.DrawString("Products:", font, XBrushes.Black, new XRect(40, 160, page.Width, page.Height), XStringFormats.TopLeft);

            int yOffset = 180;
            foreach (var product in order.Products)
            {
                graphics.DrawString($"Product ID: {product.Id}, Name: {product.Title}, Price: {product.Price} EUR", font, XBrushes.Black, new XRect(40, yOffset, page.Width, page.Height), XStringFormats.TopLeft);
                yOffset += 20;
            }

            graphics.DrawString($"Total Price: {order.TotalPrice} EUR", font, XBrushes.Black, new XRect(40, yOffset + 20, page.Width, page.Height), XStringFormats.TopLeft);

            using (var memoryStream = new MemoryStream())
            {
                document.Save(memoryStream, false);

                var base64Invoice = Convert.ToBase64String(memoryStream.ToArray());

                return base64Invoice;
            }
        }
    }
}
