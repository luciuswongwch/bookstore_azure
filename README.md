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

Test payment in Stripe  

Some of the accepted test card number  
Visa (debit)	4000056655665556	Any 3 digits	Any future date  
Mastercard	5555555555554444	Any 3 digits	Any future date  
Mastercard (2-series)	2223003122003222	Any 3 digits	Any future date  
Mastercard (debit)	5200828282828210	Any 3 digits	Any future date  
Mastercard (prepaid)	5105105105105100	Any 3 digits	Any future date  
American Express	378282246310005	Any 4 digits	Any future date  
American Express	371449635398431	Any 4 digits	Any future date  

Stripe guide of test payment  
Use a valid future date, such as 12/34.  
Use any three-digit CVC (four digits for American Express cards).  
Use any value you like for other form fields.  

List of test cards in Stripe  
https://stripe.com/docs/testing?testing-method=card-numbers#visa

---

## Deployment  

URL: https://bookstore.luciuswong.com/  

Image preview:  

![bookstore_snapshot.png](/bookstore_snapshot.png)

---

Certbot commands for HTTPS  

Dry run for certificates

```docker compose run --rm certbot certonly --webroot --webroot-path /var/www/certbot/ --dry-run -d bookstore.luciuswong.com```

Renew certificates

```$ docker compose run --rm certbot renew```

--- 

Useful documentation  

https://mindsers.blog/post/https-using-nginx-certbot-docker/