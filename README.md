# LuxeWardrobe

A modern fashion e-commerce application rebuilt from a static HTML/JavaScript storefront into a full **ASP.NET MVC 5** application using **C#, .NET Framework 4.8, Entity Framework 6, Razor, Bootstrap 5, and SQL Server LocalDB**.

The project demonstrates a complete MVC workflow with server-side cart handling, product and stock management, checkout, persisted orders, order tracking, and an admin dashboard.

## Features

### Customer storefront

- Responsive fashion storefront
- Product catalog backed by SQL Server
- Product details page
- Search products by name or description
- Filter by category and size
- Product sorting
- Per-size stock availability
- Server-side session cart
- Add, update, and remove cart items
- Promo-code support
- Delivery-charge calculation
- Checkout validation
- Server-side price and stock revalidation
- Order creation with transaction handling
- Automatic stock deduction
- Order confirmation
- Order tracking using order number and customer email

### Admin area

- Admin authentication
- Dashboard
- Add and edit products
- Show/hide products
- Manage stock for S, M, L, XL, and XXL
- View customer orders
- Update order status through:
  - Placed
  - Processing
  - Shipped
  - Delivered

## Tech Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET MVC 5 |
| Runtime | .NET Framework 4.8 |
| Language | C# |
| Views | Razor |
| ORM | Entity Framework 6 |
| Database | SQL Server LocalDB |
| Frontend | HTML5, CSS3, Bootstrap 5, JavaScript |
| Server | IIS Express / IIS |
| IDE | Visual Studio 2022 |

## Project Structure

```text
LuxeWardrobeMvc/
├── LuxeWardrobe.sln
├── .gitignore
├── README.md
├── CONVERSION_NOTES.md
│
├── LuxeWardrobe/
│   ├── App_Start/       # MVC routing and application configuration
│   ├── Content/         # CSS and product images
│   ├── Controllers/     # Store, cart, checkout, admin controllers
│   ├── Data/            # EF database context and initialization
│   ├── Filters/         # MVC filters
│   ├── Models/          # Database/domain models
│   ├── Scripts/         # JavaScript
│   ├── Services/        # Application/business services
│   ├── ViewModels/      # Strongly typed MVC view models
│   ├── Views/           # Razor views
│   ├── Global.asax
│   ├── Web.config
│   ├── packages.config
│   └── LuxeWardrobe.csproj
│
└── _OriginalStaticSource/  # Original project retained for comparison
```

## Getting Started

### Prerequisites

Install:

- Visual Studio 2022
- **ASP.NET and web development** workload
- .NET Framework 4.8 Developer Pack
- SQL Server Express LocalDB

### Run locally

1. Clone the repository.

```bash
git clone <your-repository-url>
```

2. Open:

```text
LuxeWardrobe.sln
```

3. Restore NuGet packages.

In Visual Studio, right-click the solution and select **Restore NuGet Packages** if restoration does not happen automatically.

4. Make sure `LuxeWardrobe` is the startup project.

5. Run the project using **IIS Express**.

On first run, Entity Framework creates the local database and seeds the initial LuxeWardrobe products.

## Database

The default development connection string is configured in `LuxeWardrobe/Web.config`:

```xml
Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=LuxeWardrobeMvcDb;Integrated Security=True;MultipleActiveResultSets=True
```

For production, replace this with your SQL Server or Azure SQL connection string and keep credentials outside source control.

## Demo Admin Login

For local development:

```text
Username: admin
Password: ChangeMe123!
```

> **Security:** Change the seeded admin credential before deploying the application publicly.

## Promo Code

A demo promotional code is included:

```text
LUXE10
```

It applies a 10% discount up to ₹500 when the qualifying subtotal is reached.

## Security Improvements Over the Original App

The original static application stored cart information entirely in browser `localStorage`. This MVC version moves critical operations to the server.

- Product prices are read from the database
- Stock is checked server-side
- Checkout revalidates cart quantities and prices
- Order creation runs inside a database transaction
- POST actions use anti-forgery validation
- Admin credentials are hashed instead of stored as plain text
- Customer-side values are not trusted for totals

## Original Project

The original static source is retained under `_OriginalStaticSource` for comparison with the MVC implementation.

## Future Enhancements

Useful next additions could include:

- ASP.NET Identity customer accounts
- Wishlist
- Product reviews and ratings
- Razorpay/Stripe payment gateway
- Email order confirmations
- Product image upload from the admin area
- Multiple product images
- Coupons stored in the database
- Sales/reporting dashboard
- Azure SQL deployment
- Azure App Service deployment

## Author

**Shreyjeetsinh Dodiya**

.NET Developer focused on building practical web applications with C#, ASP.NET, SQL Server, and modern web technologies.
