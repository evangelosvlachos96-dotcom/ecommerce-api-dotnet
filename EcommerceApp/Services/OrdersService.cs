using EcommerceApp.Data;
using EcommerceApp.Constants;
using EcommerceApp.Models.Database;
using EcommerceApp.Models.DTO.Base;
using EcommerceApp.Models.Requests;
using EcommerceApp.Repositories.Interfaces;
using EcommerceApp.Services.Interfaces;
using EcommerceApp.Utils;

namespace EcommerceApp.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;
        private readonly IInvoiceGeneratorService _invoiceGeneratorService;
        private readonly EcommerceContext _context;

        public OrdersService(IProductsRepository productsRepository, EcommerceContext context, IOrdersRepository ordersRepository, IInvoiceGeneratorService invoiceGeneratorService, IEmailService emailService, IUserRepository userRepository, IPaymentService paymentService)
        {
            _productsRepository = productsRepository;
            _userRepository = userRepository;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ordersRepository = ordersRepository;
            _paymentService = paymentService;
            _emailService = emailService;
            _invoiceGeneratorService = invoiceGeneratorService;
        }

        public async Task<InternalDataTransfer<Order>> CreateOrder(CreateOrderRequest request)
        {
            List<Product> products = await _productsRepository.GetProductsById(request.Products);

            if (products.Count() != request.Products.Count())
            {
                return new InternalDataTransfer<Order>(false, ProjectErrorCodes.NotExisting.GetName());
            }

            decimal totalPrice = products.Sum(p => p.Price);

            Order newOrder = new Order(request.Products, totalPrice, request.UserId)
            {
                Products = products
            };

            _ordersRepository.Add(newOrder);

            await _context.SaveChangesAsync();

            return new InternalDataTransfer<Order>(newOrder);
        }

        public async Task<InternalDataTransfer<List<Order>>> GetOrders()
        {
            List<Order> orders = await _ordersRepository.GetOrders(default);

            return new InternalDataTransfer<List<Order>>(orders);
        }

        public async Task<InternalDataTransfer<List<Order>>> GetOrderByUserId(string userId)
        {
            List<Order> orders = await _ordersRepository.GetOrderByUserId(userId, default);

            return new InternalDataTransfer<List<Order>>(orders);
        }

        public async Task<InternalDataTransfer<string>> Checkout(string userId, string identifier)
        {
            Order order = await _ordersRepository.GetOrder(userId, identifier, default);

            if (order == null)
            {
                return new InternalDataTransfer<string>(false, ProjectErrorCodes.NotExisting.GetName());
            }

            if (order.Status != Order.OrderStatus.Submitted)
            {
                return new InternalDataTransfer<string>(false, ProjectErrorCodes.InvalidInternalVerification.GetName());
            }

            User user = await _userRepository.GetById(order.UserUid);
            string email = EncryptionHelper.Decrypt(user.Email);

            var redirectUrl = _paymentService.CreateSession(order.Products.ToList(), identifier, email);

            return new InternalDataTransfer<string>(redirectUrl);
        }

        public async Task<InternalDataTransfer<bool>> ConfirmOrder(string userId, string identifier)
        {
            Order order = await _ordersRepository.GetOrder(userId, identifier, default);

            if (order == null)
            {
                return new InternalDataTransfer<bool>(false, ProjectErrorCodes.NotExisting.GetName());
            }

            if(order.Status != Order.OrderStatus.Submitted|| !string.IsNullOrEmpty(order.Invoice))
            {
                return new InternalDataTransfer<bool>(false, ProjectErrorCodes.InvalidInternalVerification.GetName());
            }

            order.MarkPaid();

            var invoiceBase64 = _invoiceGeneratorService.GenerateInvoice(order);

            var pdfBytes = Convert.FromBase64String(invoiceBase64);

            User user = await _userRepository.GetById(order.UserUid);
            string email = EncryptionHelper.Decrypt(user.Email);

            await _emailService.SendInvoiceEmailAsync(email, user.FirstName + " " + user.LastName, pdfBytes, identifier);

            order.StoreInvoice(invoiceBase64);
            await _context.SaveChangesAsync();

            return new InternalDataTransfer<bool>(true);
        }

        public async Task<InternalDataTransfer<byte[]>> GetInvoice(string userId, string identifier)
        {
            Order order = await _ordersRepository.GetOrder(userId, identifier, default);

            if (order == null || string.IsNullOrEmpty(order.Invoice))
            {
                return new InternalDataTransfer<byte[]>(false, ProjectErrorCodes.NotExisting.GetName());
            }

            var pdfBytes = Convert.FromBase64String(order.Invoice);

            return new InternalDataTransfer<byte[]>(pdfBytes);
        }

        public async Task<InternalDataTransfer<bool>> Dispatch(string userId, string identifier)
        {
            Order order = await _ordersRepository.GetOrder(userId, identifier, default);

            if (order == null)
            {
                return new InternalDataTransfer<bool>(false, ProjectErrorCodes.NotExisting.GetName());
            }

            if (order.Status != Order.OrderStatus.Paid || string.IsNullOrEmpty(order.Invoice))
            {
                return new InternalDataTransfer<bool>(false, ProjectErrorCodes.InvalidInternalVerification.GetName());
            }

            order.MarkDispatched();
            await _context.SaveChangesAsync();

            return new InternalDataTransfer<bool>(true);
        }

        public async Task<InternalDataTransfer<bool>> SendInvoice(string userId, string identifier)
        {
            Order order = await _ordersRepository.GetOrder(userId, identifier, default);

            if (order == null)
            {
                return new InternalDataTransfer<bool>(false, ProjectErrorCodes.NotExisting.GetName());
            }

            if (order.Status == Order.OrderStatus.Submitted || string.IsNullOrEmpty(order.Invoice))
            {
                return new InternalDataTransfer<bool>(false, ProjectErrorCodes.InvalidInternalVerification.GetName());
            }

            var pdfBytes = Convert.FromBase64String(order.Invoice); 
            
            User user = await _userRepository.GetById(order.UserUid);
            string email = EncryptionHelper.Decrypt(user.Email);

            await _emailService.SendInvoiceEmailAsync(email, user.FirstName + " " + user.LastName, pdfBytes, identifier);
            return new InternalDataTransfer<bool>(true);

        }
    }
}
