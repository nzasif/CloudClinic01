# CloudClinic

CloudClinic is a robust medical management platform built with an **ASP.NET Core 8.0** backend and an **Angular** frontend. This system is designed to streamline healthcare workflows including user authentication, doctor-patient management, and clinical diagnostics.


## 🚀 Key Features

### Role-Based Modules
The system is architected into specialized modules to manage clinical and administrative workflows:
* **DrAccountant**: Manages financial transactions, including payment viewing and confirmation.
* **DrAssistant**: Handles appointment scheduling, searches, rejection/un-rejection, and practice time/holiday configuration.
* **DrCheckPat**: The clinical core, managing patient reports, prescriptions, and referrals for labs/X-rays.
* **DrLabStaff**: Manages lab test referrals and result submission with file attachments.
* **DrPharmacyStaff**: Manages drug inventory and prescription tracking.
* **DrFeedback/Details/Progress**: Tools for communication, profile management, and clinical performance analytics.

## 🛠 Technical Details
* **Framework**: ASP.NET Core 8.0, Entity Framework Core, SQL Server.
* **API Documentation**: Swagger/OpenAPI 3.0.1 integrated.
* **Database Strategy**: Circular deletion dependencies resolved using `DeleteBehavior.Restrict`.
* **Security**: JWT-based authentication and role-based access control.

## 📋 Setup & Maintenance

### Initialization
1. Ensure .NET 8.0 SDK and SQL Server are installed.
2. Configure `appsettings.json` with your connection string.
3. Apply database schema: `dotnet ef database update`.

### Maintenance Protocols
* **Clean Build**: `dotnet clean` and remove `bin`/`obj` folders.
* **Cache Refresh**: `dotnet nuget locals all --clear`.
* **Migration Reset**: 
    1. Delete `Migrations/` folder.
    2. `dotnet ef migrations add InitialFullMigration`.
    3. `dotnet ef database update`.

### 🖥 Angular Client App
The frontend is a modular Single Page Application (SPA) utilizing:
* **Service-Oriented Design**: Dedicated services for each clinical domain (Lab, Pharmacy, Assistant).
* **Type Safety**: TypeScript models synchronized with C# DTOs.
* **Authentication Interceptor**: Automated JWT injection for secure API communication.
* **Feature-Based Routing**: Clean separation of clinical dashboards and management consoles.

## 📄 Documentation
For detailed API operation IDs, helper methods, and deployment checklists, please refer to the `CloudClinic_Full_Documentation.txt` file included in this repository.

## 🤝 Contributing
Contributions are welcome. Please submit a pull request with a clear description of your changes.# CloudClinic