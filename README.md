User roles in the application
- individual (default)
- company
- employee
- admin

---

Individual role (default user role):
- perform basic functions of any ecommerce application (e.g. add to shopping cart, edit profile, and checkout)
- can only view the orders placed by themselves

Company role:
- different from individual role in being allowed to have delayed payment

Employee role:
- authorized to view and edit orders placed by other users
- can edit order status (e.g. from confirmed to processing, or from processing to shipped) of all orders

Admin role:
- can perform all actions authorized to the employee role
- authorized to perform crud operations to categories, cover types and products
- authorized to add users with other roles including admin role

---

Order status in the application
- pending (default order status when order is unpaid)
- confirmed (order status when payment is received, except for company users as delayed payment is allowed)*
- processing
- shipped
- cancelled

\* order status of company user is by default confirmed without payment

Payment status in the application
- notReceived
- received
- refunded (if paid order is being cancelled)

---

Integrations with third-party services:
- MailKit and MimeKit for email sender
- Stripe payment for checkout

appsettings.json is included in .gitignore file to hide configuration strings  
SeedData.cs is included in .gitignore file to hide the login credentials of seeded users

---

## Deployment  

Live preview: https://bookstoreluciuswong.azurewebsites.net/  

Image preview:  

![bookstore_snapshot.png](/bookstore_snapshot.png)