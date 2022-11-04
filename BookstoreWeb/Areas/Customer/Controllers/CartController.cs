using Bookstore.DataAccess;
using Bookstore.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IOptions<StripeSettings> _stripeSettings;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public int OrderTotal { get; set; }
        public CartController(UserManager<ApplicationUser> userManager, ApplicationDbContext db, IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _db = db;
            _stripeSettings = stripeSettings;
        }
        public IActionResult ShoppingCart()
        {
            var currentUserId = _userManager.GetUserId(User);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _db.ShoppingCarts.Include(u => u.Product).Where(u => u.ApplicationUserId == currentUserId),
                OrderHeader = new OrderHeader()
            };

            if (ShoppingCartVM.ListCart.Count() == 0)
            {
                TempData["info"] = "Please add item to shipping cart before proceeding";
                return RedirectToAction("Index", "Home");
            }

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Quantity, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Quantity);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult OrderSummary()
        {
            var currentUserId = _userManager.GetUserId(User);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _db.ShoppingCarts.Include(u => u.Product)
                            .Where(u => u.ApplicationUserId == currentUserId),
                OrderHeader = new OrderHeader()
            };

            if (ShoppingCartVM.ListCart.Count() == 0)
                return RedirectToAction("Index", "Home");

            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == currentUserId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Quantity, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Quantity);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("OrderSummary")]
        [ValidateAntiForgeryToken]
        public IActionResult OrderSummaryPost()
        {
            var currentUserId = _userManager.GetUserId(User);

            ShoppingCartVM.ListCart = _db.ShoppingCarts.Include(u => u.Product).Where(u => u.ApplicationUserId == currentUserId);

            if (ShoppingCartVM.ListCart.Count() == 0)
                return RedirectToAction("Index", "Home");

            ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == currentUserId);
            if (applicationUser == null)
            {
                TempData["warning"] = "Session expired. Please login in again before checkout";
                return RedirectToAction("Index", "Home");
            }

            // Update user profile based on the shopping cart details
            applicationUser.PhoneNumber = ShoppingCartVM.OrderHeader.PhoneNumber;
            applicationUser.StreetAddress = ShoppingCartVM.OrderHeader.StreetAddress;
            applicationUser.City = ShoppingCartVM.OrderHeader.City;
            applicationUser.State = ShoppingCartVM.OrderHeader.State;
            applicationUser.PostalCode = ShoppingCartVM.OrderHeader.PostalCode;
            _db.SaveChanges();

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = currentUserId;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Quantity, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Quantity);
            }
            _db.SaveChanges();

            // Run if they are not company user
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = BookstoreConstant.PaymentStatusNotReceived;
                ShoppingCartVM.OrderHeader.OrderStatus = BookstoreConstant.StatusPending;
            }
            // Allow delayed payment if they are company user
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = BookstoreConstant.PaymentStatusNotReceived;
                ShoppingCartVM.OrderHeader.OrderStatus = BookstoreConstant.StatusConfirmed;
            }

            _db.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
            _db.SaveChanges();

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };
                _db.OrderDetails.Add(orderDetail);
            }
            _db.SaveChanges();

            // Direct to stripe payment if they are not company user
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = _stripeSettings.Value.Domain;
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = _stripeSettings.Value.CheckoutMode,
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"admin/order/index?status=all",
                };

                foreach (var item in ShoppingCartVM.ListCart)
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

                ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                ShoppingCartVM.OrderHeader.SessionId = session.Id;
                ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;
                _db.SaveChanges();

                // Remove shopping cart data and clear session
                HttpContext.Session.Clear();
                _db.ShoppingCarts.RemoveRange(ShoppingCartVM.ListCart);
                _db.SaveChanges();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            else
            {
                // Remove shopping cart data and clear session
                HttpContext.Session.Clear();
                _db.ShoppingCarts.RemoveRange(ShoppingCartVM.ListCart);
                _db.SaveChanges();

                return RedirectToAction(nameof(OrderConfirmation), "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
            }
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _db.OrderHeaders.Include(u => u.ApplicationUser).First(u => u.Id == id);
            if (_userManager.GetUserId(User) != orderHeader.ApplicationUserId)
            {
                TempData["warning"] = "Session expired. To confirm payment, please log in as the user who placed the order.";
                return RedirectToAction("Index", "Home");
            }

            // check stripe payment if they are not company user
            if (orderHeader.ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    orderFromDb.PaymentIntentId = session.PaymentIntentId;
                    orderFromDb.OrderStatus = BookstoreConstant.StatusConfirmed;
                    orderFromDb.PaymentStatus = BookstoreConstant.PaymentStatusReceived;
                    _db.SaveChanges();
                }
                else
                {
                    TempData["info"] = "Payment has not be made. Please consider to make the payment on the order page.";
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(id);
        }

            public IActionResult Plus(int cartId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(u => u.Id == cartId);
            cart.Quantity += 1;
            _db.SaveChanges();
            return RedirectToAction(nameof(ShoppingCart));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(u => u.Id == cartId);
            if (cart.Quantity > 1)
            {
                cart.Quantity -= 1;
            } else
            {
                TempData["warning"] = "Item count is already 1, click on the delete button to remove this item";
            }
            _db.SaveChanges();


            return RedirectToAction(nameof(ShoppingCart));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _db.ShoppingCarts.FirstOrDefault(u => u.Id == cartId);
            _db.ShoppingCarts.Remove(cart);
            _db.SaveChanges();

            TempData["info"] = "Item is removed from the shopping cart";

            var count = _db.ShoppingCarts.Where(u => u.ApplicationUserId == cart.ApplicationUserId).Count();
            HttpContext.Session.SetInt32(BookstoreConstant.SessionCart, count);

            return RedirectToAction(nameof(ShoppingCart));
        }

        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else if (quantity >= 51 && quantity <= 100)
            {
                return price50;
            }
            else
            {
                return price100;
            }
        }
    }
}
