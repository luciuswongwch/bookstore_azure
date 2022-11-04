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
	public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
	{
		private ApplicationDbContext _db;

		public ShoppingCartRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}

        public int IncrementQuantity(ShoppingCart shoppingCart, int quantity)
        {
			shoppingCart.Quantity += quantity;
			return shoppingCart.Quantity;
        }

		public int DecrementQuantity(ShoppingCart shoppingCart, int quantity)
		{
			shoppingCart.Quantity -= quantity;
			return shoppingCart.Quantity;
		}
	}
}
