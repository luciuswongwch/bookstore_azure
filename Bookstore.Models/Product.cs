using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Models
{
	public class Product
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string Title { get; set; }
		public string Description { get; set; }
		[Required]
		public string ISBN { get; set; }
		[Required]
		public string Author { get; set; }
		[Required]
		[Range(1, 10000)]
		[Display(Name = "List price (CAD)")]
		public double ListPrice { get; set; }
		[Required]
		[Range(1, 10000)]
		[Display(Name = "Unit price (CAD) for quantity 1-50")]
		public double Price { get; set; }
		[Required]
		[Range(1, 10000)]
		[Display(Name ="Unit price (CAD) for quantity 51-100")]
		public double Price50 { get; set; }
		[Required]
		[Range(1, 10000)]
		[Display(Name ="Unit price (CAD) for quantity 100+")]
		public double Price100 { get; set; }
		[ValidateNever]
		public string ImageUrl { get; set; }
		[Required]
		[DisplayName("Category")]
		public int CategoryId { get; set; }
		[ForeignKey("CategoryId")]
		[ValidateNever]
		public Category Category { get; set; }

		[Required]
		[DisplayName("Cover")]
		public int CoverId { get; set; }
		[ForeignKey("CoverId")]
		[ValidateNever]
		public Cover Cover { get; set; }
	}
}
