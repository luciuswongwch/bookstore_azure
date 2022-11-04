using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Utility
{
	public static class BookstoreConstant
	{
		public const string Role_User_Individual = "individual";
		public const string Role_User_Company = "company";
		public const string Role_User_Admin = "admin";
		public const string Role_User_Employee = "employee";

        // default order status except company users
        public const string StatusPending = "pending";
        // when order is paid by individual users, or when order is made by company users
        public const string StatusConfirmed = "confirmed";
        // can be changed by admin or employee users in order details page
        public const string StatusProcessing = "processing";
        // can be changed by admin or employee users in order details page
        public const string StatusShipped = "shipped";
        public const string StatusCancelled = "cancelled";

        public const string PaymentStatusNotReceived = "notReceived";
        public const string PaymentStatusReceived = "received";
        public const string PaymentStatusRefunded = "refunded";

        public const string SessionCart = "sessionShoppingCart";
    }
}
