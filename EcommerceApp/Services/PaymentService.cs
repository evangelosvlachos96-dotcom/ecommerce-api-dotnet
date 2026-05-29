using EcommerceApp.Models.Database;
using EcommerceApp.Services.Interfaces;
using Stripe.Checkout;

namespace EcommerceApp.Services
{
    public class PaymentService : IPaymentService
    {
        public static readonly string WebUrl = "https://localhost:7063/api";
        public string CreateSession(List<Product> productList, string orderId, string email)
        {
            var successUrl = $"{WebUrl}/orders/checkout-confirm?identifier={orderId}";

            var options = new SessionCreateOptions
            {
                SuccessUrl = successUrl,
                // Cancel page can be created on the front or as a razorpage
                CancelUrl = WebUrl + "CheckOut/Login",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                CustomerEmail = email
            };

            foreach (var product in productList)
            {
                var sessionListItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(product.Price * 100),
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = product.Title
                        }
                    },
                    Quantity = 1
                };
                options.LineItems.Add(sessionListItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            return session.Url;
        }
    }
}
