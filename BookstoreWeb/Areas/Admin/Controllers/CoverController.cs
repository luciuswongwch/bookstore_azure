using Bookstore.DataAccess;
using Bookstore.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = BookstoreConstant.Role_User_Admin)]
    public class CoverController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CoverController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Cover> objCoverList = _db.Covers;
            return View(objCoverList);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cover obj)
        {
            if (ModelState.IsValid)
            {
                _db.Covers.Add(obj);
                _db.SaveChanges();
                TempData["info"] = "Cover is created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverFromDb = _db.Covers.Find(id);
            if (coverFromDb == null)
            {
                return NotFound();
            }
            return View(coverFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Cover obj)
        {
            if (ModelState.IsValid)
            {
                _db.Covers.Update(obj);
                _db.SaveChanges();
                TempData["info"] = "Cover is updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverFromDb = _db.Covers.Find(id);
            if (coverFromDb == null)
            {
                return NotFound();
            }
            return View(coverFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Covers.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Covers.Remove(obj);
            _db.SaveChanges();
            TempData["info"] = "Cover is deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
