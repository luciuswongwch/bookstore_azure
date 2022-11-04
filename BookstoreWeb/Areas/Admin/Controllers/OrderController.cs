using Bookstore.DataAccess;
using Bookstore.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IOptions<StripeSettings> _stripeSettings;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(UserManager<ApplicationUser> userManager, ApplicationDbContext db, IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _db = db;
            _stripeSettings = stripeSettings;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _db.OrderHeaders.Include(u => u.ApplicationUser).FirstOrDefault(u => u.Id == orderId),
                OrderDetail = _db.OrderDetails.Include(u => u.Product).Where(u => u.OrderId == orderId)
            };
            if (OrderVM.OrderHeader.ApplicationUserId == _userManager.GetUserId(User)
                || User.IsInRole(BookstoreConstant.Role_User_Admin)
                || User.IsInRole(BookstoreConstant.Role_User_Employee))
            {
                return View(OrderVM);
            } 
            else
            {
                TempData["warning"] = "Access denied. Please log in as the user who placed the order to proceed.";
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StripePayNow()
        {
            OrderVM.OrderHeader = _db.OrderHeaders.Include(u => u.ApplicationUser).FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            OrderVM.OrderDetail = _db.OrderDetails.Include(u => u.Product).Where(u => u.OrderId == OrderVM.OrderHeader.Id);

            if (OrderVM.OrderHeader.ApplicationUserId != _userManager.GetUserId(User))
            {
                TempData["warning"] = "Access denied. Please log in as the user who placed the order to proceed.";
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            // Stripe payment
            string domain = _stripeSettings.Value.Domain;
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = _stripeSettings.Value.CheckoutMode,
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long) item.Price * 100,
                        Currency = _stripeSettings.Value.Currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        },
                    },
                    Quantity = item.Quantity
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            OrderVM.OrderHeader.PaymentDate = DateTime.Now;
            OrderVM.OrderHeader.SessionId = session.Id;
            OrderVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;
            _db.SaveChanges();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderHeaderId);
            if (orderHeader.ApplicationUserId != _userManager.GetUserId(User))
            {
                TempData["warning"] = "Access denied. Please log in as the user who placed the order to proceed.";
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderHeaderId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                orderFromDb.PaymentIntentId = session.PaymentIntentId;
                orderFromDb.PaymentStatus = BookstoreConstant.PaymentStatusReceived;
                orderFromDb.OrderStatus = BookstoreConstant.StatusConfirmed;
                _db.SaveChanges();
            } 
            else
            {
                TempData["info"] = "Payment has not be made. Please consider to make the payment on the order page.";
                return RedirectToAction("Index", "Order", new { status = "all" });
            }
            return View(orderHeaderId);
        }

        [HttpPost]
        [Authorize(Roles = BookstoreConstant.Role_User_Admin + "," + BookstoreConstant.Role_User_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _db.OrderHeaders.AsNoTracking().FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (orderHeaderFromDb.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (orderHeaderFromDb.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _db.OrderHeaders.Update(orderHeaderFromDb);
            _db.SaveChanges();

            TempData["info"] = "Order details updated successfully";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = BookstoreConstant.Role_User_Admin + "," + BookstoreConstant.Role_User_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            var orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.OrderStatus = BookstoreConstant.StatusProcessing;
            _db.SaveChanges();
            TempData["info"] = "Order status is updated to processing. Order is going to be processed.";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = BookstoreConstant.Role_User_Admin + "," + BookstoreConstant.Role_User_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _db.OrderHeaders.Include(u => u.ApplicationUser).FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeaderFromDb.PaymentStatus != BookstoreConstant.PaymentStatusReceived)
            {
                // allow company user to pay the order within 30 days the order is shipped
                if (orderHeaderFromDb.ApplicationUser.CompanyId.GetValueOrDefault() != 0)
                {
                    orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
                }
                else
                {
                    return RedirectToAction("Index", "Order", new { status = "all" });
                }
            }
            orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = BookstoreConstant.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            _db.SaveChanges();
            TempData["info"] = "Order shipped successfully. Order status is updated to shipped.";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = BookstoreConstant.Role_User_Admin + "," + BookstoreConstant.Role_User_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb.PaymentStatus == BookstoreConstant.PaymentStatusReceived)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                orderHeaderFromDb.PaymentStatus = BookstoreConstant.PaymentStatusRefunded;
            }
            orderHeaderFromDb.OrderStatus = BookstoreConstant.StatusCancelled;
            _db.SaveChanges();
            TempData["info"] = "Order cancelled successfully";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderHeaderFromDb.Id });
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            orderHeaders = _db.OrderHeaders.Include(u => u.ApplicationUser);

            if (!User.IsInRole(BookstoreConstant.Role_User_Admin) && !User.IsInRole(BookstoreConstant.Role_User_Employee))
            {
                orderHeaders = orderHeaders.Where(u => u.ApplicationUserId == _userManager.GetUserId(User));
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == BookstoreConstant.StatusPending);
                    break;
                case "confirmed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == BookstoreConstant.StatusConfirmed);
                    break;
                case "processing":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == BookstoreConstant.StatusProcessing);
                    break;
                case "shipped":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == BookstoreConstant.StatusShipped);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
