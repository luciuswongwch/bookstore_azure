using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Utility
{
	public class StripeSettings
	{
		public string SecretKey { get; set; }
		public string PublishableKey { get; set; }
		public string Currency { get; set; }
		public string CheckoutMode { get; set; }
		public string Domain { get; set; }
	}
}
