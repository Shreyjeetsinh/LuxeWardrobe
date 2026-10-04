# Conversion notes

## Original project
The uploaded version was a static client-side storefront with:
- `index.html`
- `cart.html`
- browser `localStorage` cart logic
- three AVIF product images
- no server, authentication, database, persisted orders, or stock enforcement

## MVC 5 version
The converted project targets **.NET Framework 4.8 + ASP.NET MVC 5 + Entity Framework 6**.

### Added architecture
- Controllers: Home, Products, Cart, Checkout, Order, Admin, AdminProducts
- EF6 data context + first-run seed initializer
- Models for Product, ProductSizeInventory, Order, OrderItem, AdminUser
- Session cart service and centralized server-side pricing service
- Admin authorization filter
- Strongly typed Razor views and a shared layout

### Added user features
- Search, category filtering, size filtering, and sorting
- Product details and size-specific stock availability
- Session-based cart with quantity update/remove
- `LUXE10` promotional discount
- Delivery calculation
- Checkout validation
- Database order creation and stock deduction in a transaction
- Order confirmation and order tracking

### Added admin features
- Local admin sign-in
- Dashboard metrics
- Product create/edit/hide
- Stock editing for S/M/L/XL/XXL
- Order list and fulfillment status updates

### Security/data integrity improvements
- Prices are never trusted from the browser
- Product availability and stock are checked on the server
- Stock is checked again at checkout
- State-changing forms use anti-forgery tokens
- Admin passwords are salted + PBKDF2 hashed
- Checkout uses a DB transaction so order + inventory updates stay consistent

## Important
The seeded admin account (`admin` / `ChangeMe123!`) is for local development only. Replace it before public deployment.
