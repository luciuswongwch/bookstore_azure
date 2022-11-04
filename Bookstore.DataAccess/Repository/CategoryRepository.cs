using Bookstore.DataAccess.Repository.IRepository;
using Bookstore.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.DataAccess.Repository
{
	public class CategoryRepository : Repository<Category>, ICategoryRepository
	{
		private ApplicationDbContext _db;

		public CategoryRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}

		void ICategoryRepository.Update(Category obj)
		{
			_db.Categories.Update(obj);
		}
	}
}
