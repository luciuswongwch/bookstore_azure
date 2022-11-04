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
	public class CoverRepository : Repository<Cover>, ICoverRepository
	{
		private ApplicationDbContext _db;

		public CoverRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}

		void ICoverRepository.Update(Cover obj)
		{
			_db.Covers.Update(obj);
		}
	}
}
