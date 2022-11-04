using Bookstore.DataAccess;
using Bookstore.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookstoreWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public ShoppingCartViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync() {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(BookstoreConstant.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(BookstoreConstant.SessionCart,
                        _db.ShoppingCarts.Where(u => u.ApplicationUserId == claim.Value).Count());
                }
                return View(HttpContext.Session.GetInt32(BookstoreConstant.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
