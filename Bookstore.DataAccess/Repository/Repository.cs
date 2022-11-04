using Bookstore.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.DataAccess.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _db;
		internal DbSet<T> dbSet;

		public Repository(ApplicationDbContext db)
		{
			_db = db;
			this.dbSet = _db.Set<T>();
		}

		void IRepository<T>.Add(T entity)
		{
			dbSet.Add(entity);
		}

		IEnumerable<T> IRepository<T>.GetAll(string? includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
			return query.ToList();
		}

		T IRepository<T>.GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			query = query.Where(filter);
			if (includeProperties != null)
			{
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
			return query.FirstOrDefault(filter); ;
		}

		void IRepository<T>.Remove(T entity)
		{
			dbSet.Remove(entity);
		}

		void IRepository<T>.RemoveRange(IEnumerable<T> entity)
		{
			dbSet.RemoveRange(entity);
		}
	}
}
