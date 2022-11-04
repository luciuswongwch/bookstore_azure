using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Models
{
	public class Cover
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }
	}
}
