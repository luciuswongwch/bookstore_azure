using Bookstore.DataAccess;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_db.Products.Include(u=>u.Category).Include(u=>u.Cover));
        }

        public IActionResult ProductDetails(int productId)
        {
            ShoppingCart cartObj = new()
            {
                ProductId = productId,
                Product = _db.Products.Include(u => u.Category).Include(u => u.Cover)
                            .FirstOrDefault(u => u.Id == productId),
                Quantity = 1
            };
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ProductDetails(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartFromDb = _db.ShoppingCarts.FirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

            if (cartFromDb == null)
            {
                _db.ShoppingCarts.Add(shoppingCart);
            }
            else
            {
                cartFromDb.Quantity += shoppingCart.Quantity;
            }
            _db.SaveChanges();

            TempData["success"] = "Item is added to the shopping cart successfully";

            HttpContext.Session.SetInt32(BookstoreConstant.SessionCart,
                    _db.ShoppingCarts.Where(u => u.ApplicationUserId == claim.Value).Count());

            return RedirectToAction(nameof(ProductDetails), new { productId = shoppingCart.ProductId});
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}