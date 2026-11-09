# Inventory & Sales Management System

A Windows desktop application for managing products, recording sales and tracking low stock. Built with **C#**, **Windows Forms** and **SQL Server LocalDB**.


---

## Features

### Login and account recovery
- Secure login with hashed passwords (PBKDF2 + salt, never stored in plain text)
- Forgot password flow: verify email, receive a reset code, set a new password
- Default admin account created automatically on first run

### Product management
- Add, update, delete and search products
- Fields: Product ID, Name, Category, Price, Quantity, Reorder Level
- Input validation (price must be above 0, quantity cannot be negative)
- Deletion is blocked for products that already have sales records

### Sales management
- Enter a product ID and quantity, then confirm the sale
- Checks that the product exists and that stock is sufficient
- Calculates the total amount automatically
- Saves the sale, sale details and updated stock in a single database transaction (all or nothing)
- Shows a low-stock alert when a sale drops a product to its reorder level

### Low stock management
- Lists every product at or below its reorder level
- Shows a summary count, or "No low-stock products found"

---

## Tech stack

| Layer | Technology |
|---|---|
| Language | C# |
| UI | Windows Forms (.NET) |
| Database | SQL Server LocalDB |
| Data access | Microsoft.Data.SqlClient (parameterized queries) |
| IDE | Visual Studio 2022 |

---

## Project structure

```
inventory_mangement_system/
├── Program.cs        # Entry point: initializes DB, shows login, opens main window
├── Db.cs             # Database helper, auto-creates DB and tables, password hashing
├── LoginForm.cs      # Login screen
├── ForgotForm.cs     # Password reset screen
├── MainForm.cs       # Tabs: Products, Sales, Low Stock
└── inventory_mangement_system.csproj
```

---

## Database design

The database `InventoryDB` and all tables are **created automatically** on first run. No SQL script is needed.

| Table | Purpose |
|---|---|
| `Users` | Username, email, password hash, reset code |
| `Products` | ProductID, ProductName, Category, Price, Quantity, ReorderLevel |
| `Sales` | SaleID, SaleDate, TotalAmount |
| `SaleDetails` | SaleID, ProductID, Qty, UnitPrice |

---

## Getting started

### Prerequisites
- Windows 10 or 11
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with these workloads:
  - **.NET desktop development**
  - **Data storage and processing** (includes SQL Server LocalDB)
- Git

### Run locally
```bash
git clone https://github.com/<wimansi-navoda>/<Inventory_Management_System>.git
cd <Inventory_Management_System>
```

1. Open the `.sln` file in Visual Studio.
2. Restore NuGet packages (this happens automatically on build). If needed, run in the Package Manager Console:
   ```powershell
   Install-Package Microsoft.Data.SqlClient
   ```
3. Set the build configuration to **Debug**.
4. Press **F5** to run.

### Default login

| Username | Password |
|---|---|
| `admin` | `admin123` |

Change this password after your first login.

---

## Usage

1. **Log in** with the admin account.
2. **Products tab:** add products, click a row to edit it, or use the search box.
3. **Sales tab:** enter a Product ID and quantity, click **Sell**, then confirm.
4. **Low Stock tab:** view every product that needs reordering.

---

## Deployment

Publish a self-contained single-file executable:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output is in `bin\Release\<target-framework>\win-x64\publish\`.

The target PC needs **SQL Server LocalDB** or **SQL Server Express**. To use a shared server, change the connection string in `Db.cs`:

```csharp
"Server=SERVERNAME\\SQLEXPRESS;Database=InventoryDB;User Id=...;Password=...;TrustServerCertificate=True;"
```

---

## Security notes

- Passwords are hashed with PBKDF2 (SHA-256, 100,000 iterations) and a random salt.
- All SQL uses parameterized queries to prevent SQL injection.
- The forgot-password reset code is shown on screen for **demo purposes only**. A production system should email it (for example with SMTP or SendGrid).

---

## Future improvements

- Email delivery for password reset codes
- Sign in with Google
- Sales reports and printable receipts
- Multiple user roles (admin, cashier)
- Export products and sales to Excel or PDF

---

## Author

wimansi
GitHub: [@your-username]((https://github.com/wimansi-navoda))

---

## License

This project is for educational purposes. Add a license (for example MIT) if you plan to share it publicly.
