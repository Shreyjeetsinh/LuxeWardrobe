# LuxeWardrobe — ASP.NET MVC 5 conversion

This solution converts the original static HTML/JavaScript LuxeWardrobe storefront into a **C# ASP.NET MVC 5 application targeting .NET Framework 4.8**.

## What changed

- MVC 5 controllers, strongly typed Razor views, shared layout and separated CSS/JS
- Entity Framework 6 + SQL Server LocalDB persistence
- Product catalog with search, category filter, size filter and sorting
- Product details with per-size stock
- Server-side Session cart (no client-trusted price data)
- Stock and price revalidation during checkout
- Promo code support (`LUXE10`: 10% off up to ₹500, minimum subtotal ₹1,500)
- Free delivery above ₹2,000; otherwise ₹20
- Checkout with validation and persisted orders/order items
- Automatic stock deduction inside a database transaction
- Order confirmation and customer order tracking
- Admin dashboard, product management, stock-by-size editing and order status updates
- Anti-forgery tokens on state-changing POST actions
- Modern responsive Bootstrap 5 UI

## Run in Visual Studio

1. Open `LuxeWardrobe.sln` in **Visual Studio 2022** on Windows.
2. Ensure the **.NET Framework 4.8 Developer Pack** and **ASP.NET and web development** workload are installed.
3. Restore NuGet packages when Visual Studio prompts you (or right-click the solution → Restore NuGet Packages).
4. Make sure **SQL Server Express LocalDB** is installed.
5. Run with IIS Express.
6. On first run, EF6 creates the LocalDB database named `LuxeWardrobeMvcDb` and seeds the three original products.

## Admin demo login

- Username: `admin`
- Password: `ChangeMe123!`

**Important:** this is only a seeded local-development credential. Replace it before deploying the application publicly.

## Database connection

The default connection string is in `Web.config`:

```xml
Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=LuxeWardrobeMvcDb;Integrated Security=True;MultipleActiveResultSets=True
```

You can replace it with an Azure SQL or SQL Server connection string without changing the controllers.

## Original assets retained

The original `t-1.avif`, `t-2.avif`, and `t-3.avif` product images are copied into `Content/images/products`.
