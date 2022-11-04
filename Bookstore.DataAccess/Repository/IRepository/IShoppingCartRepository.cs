using Bookstore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.DataAccess.Repository.IRepository
{
	public interface IShoppingCartRepository : IRepository<ShoppingCart>
	{
		int IncrementQuantity(ShoppingCart shoppingCart, int quantity);
		int DecrementQuantity(ShoppingCart shoppingCart, int quantity);
	}
}
