using Bookstore.DataAccess;
using Bookstore.Models.ViewModels;
using Bookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Primitives;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = BookstoreConstant.Role_User_Admin)]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment; 
        }
        public IActionResult Index()
        {
            IEnumerable<Product> objProductList = _db.Products;
            return View(objProductList);
        }

        public IActionResult UpdateOrInsertIfNotExist(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _db.Categories.ToList().Select(
                    i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                CoverList = _db.Covers.ToList().Select(
                    i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    })
            };
            if (id == 0 || id == null)
            {
                return View(productVM);
            } else
            {
                productVM.Product = _db.Products.FirstOrDefault(u=>u.Id == id);

                return View(productVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrInsertIfNotExist(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                if (obj.Product.Id == 0)
                {
                    _db.Products.Add(obj.Product);
                    TempData["info"] = "Product is created successfully";
                } else
                {
                    _db.Products.Update(obj.Product);
                    TempData["info"] = "Product is updated successfully";
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            obj.CategoryList = _db.Categories.ToList().Select(
                    i => new SelectListItem {
                        Text = i.Name,
                        Value = i.Id.ToString()});
            obj.CoverList = _db.Covers.ToList().Select(
                    i => new SelectListItem {
                        Text = i.Name,
                        Value = i.Id.ToString()});
            return View(obj);
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll() {

            var query = _db.Products.Include(u => u.Category).Include(u => u.Cover);

            return Json(new { data = query.ToList() });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Products.Find(id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _db.Products.Remove(obj);
            _db.SaveChanges();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}
