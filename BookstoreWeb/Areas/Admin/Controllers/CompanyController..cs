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
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CompanyController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Company> objCompanyList = _db.Companies;
            return View(objCompanyList);
        }

        public IActionResult UpdateOrInsertIfNotExist(int? id)
        {
            Company company = new Company();
            
            if (id == 0 || id == null)
            {
                return View(company);
            } 
            else
            {
                company = _db.Companies.FirstOrDefault(u => u.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrInsertIfNotExist(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _db.Companies.Add(obj);
                    TempData["info"] = "Company is created successfully";
                } else
                {
                    _db.Companies.Update(obj);
                    TempData["info"] = "Company is updated successfully";
                }
                _db.SaveChanges();
                
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll() {
            return Json(new { data = _db.Companies.ToList() });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Companies.Find(id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _db.Companies.Remove(obj);
            _db.SaveChanges();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}
